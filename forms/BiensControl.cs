using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Windows.Forms;

public class BiensControl : UserControl
{
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly TextBox txtRef = new(), txtAdresse = new(), txtVille = new(), txtCP = new(), txtRecherche = new();
    private readonly ComboBox cmbType = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbStatut = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown numSurface = new() { DecimalPlaces = 1, Maximum = 100000 };
    private readonly NumericUpDown numPieces = new() { Maximum = 100 };
    private readonly NumericUpDown numLoyer = new() { DecimalPlaces = 2, Maximum = 10000000 };

    private int? selectedId = null;

    public BiensControl()
    {
        cmbType.DataSource = Enum.GetValues<TypeBien>();
        cmbStatut.DataSource = Enum.GetValues<StatutBien>();

        // Zone de recherche
        var pnlRecherche = new Panel { Dock = DockStyle.Top, Height = 40 };
        pnlRecherche.Controls.Add(new Label { Text = "Rechercher (référence ou ville) :", Left = 10, Top = 12, AutoSize = true });
        txtRecherche.Left = 230; txtRecherche.Top = 8; txtRecherche.Width = 250;
        txtRecherche.TextChanged += (s, e) => Charger();
        pnlRecherche.Controls.Add(txtRecherche);

        // Zone du formulaire
        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 170 };
        Champ(pnlForm, "Référence", txtRef, 10, 10, 140);
        Champ(pnlForm, "Type", cmbType, 170, 10, 140);
        Champ(pnlForm, "Surface (m²)", numSurface, 330, 10, 120);
        Champ(pnlForm, "Nb pièces", numPieces, 470, 10, 100);
        Champ(pnlForm, "Loyer de base (DT)", numLoyer, 590, 10, 140);
        Champ(pnlForm, "Adresse", txtAdresse, 10, 70, 300);
        Champ(pnlForm, "Ville", txtVille, 330, 70, 140);
        Champ(pnlForm, "Code postal", txtCP, 490, 70, 100);
        Champ(pnlForm, "Statut", cmbStatut, 610, 70, 120);

        Bouton(pnlForm, "Ajouter", 10, 130, (s, e) => Ajouter());
        Bouton(pnlForm, "Modifier", 120, 130, (s, e) => Modifier());
        Bouton(pnlForm, "Supprimer", 230, 130, (s, e) => Supprimer());
        Bouton(pnlForm, "Vider", 340, 130, (s, e) => Vider());

        // Clic sur une ligne = remplit le formulaire
        grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                Selectionner(Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value));
        };

        // Ordre important : le tableau d'abord, puis la recherche, puis le formulaire
        Controls.Add(grid);
        Controls.Add(pnlRecherche);
        Controls.Add(pnlForm);

        Charger();
    }

    // ---------- Outils d'affichage ----------
    private static void Champ(Panel p, string titre, Control c, int x, int y, int largeur)
    {
        p.Controls.Add(new Label { Text = titre, Left = x, Top = y, AutoSize = true });
        c.Left = x; c.Top = y + 20; c.Width = largeur;
        p.Controls.Add(c);
    }

    private static void Bouton(Panel p, string texte, int x, int y, EventHandler action)
    {
        var b = new Button { Text = texte, Left = x, Top = y, Width = 100, Height = 30 };
        b.Click += action;
        p.Controls.Add(b);
    }

    private static void Erreur(string message)
        => MessageBox.Show(message, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // ---------- Accès à la base (Entity Framework) ----------
    private void Charger()
    {
        using var db = new AppDbContext();
        var q = db.Biens.AsQueryable();

        var t = txtRecherche.Text.Trim();
        if (t != "")
            q = q.Where(b => b.Reference.Contains(t) || b.Localisation!.Ville.Contains(t));

        grid.DataSource = q.OrderBy(b => b.Reference)
            .Select(b => new
            {
                b.Id,
                b.Reference,
                b.Type,
                b.Surface,
                b.NbPieces,
                b.LoyerBase,
                b.Statut,
                Adresse = b.Localisation!.Adresse,
                Ville = b.Localisation!.Ville
            })
            .ToList();

        if (grid.Columns["Id"] != null)
            grid.Columns["Id"]!.Visible = false;
    }

    private void Selectionner(int id)
    {
        using var db = new AppDbContext();
        var b = db.Biens.Include(x => x.Localisation).FirstOrDefault(x => x.Id == id);
        if (b == null) return;

        selectedId = b.Id;
        txtRef.Text = b.Reference;
        cmbType.SelectedItem = b.Type;
        numSurface.Value = (decimal)b.Surface;
        numPieces.Value = b.NbPieces;
        numLoyer.Value = b.LoyerBase;
        cmbStatut.SelectedItem = b.Statut;
        txtAdresse.Text = b.Localisation?.Adresse ?? "";
        txtVille.Text = b.Localisation?.Ville ?? "";
        txtCP.Text = b.Localisation?.CodePostal ?? "";
    }

    // Remplit un bien avec les valeurs du formulaire (et réutilise la localisation si elle existe déjà)
    private void Remplir(AppDbContext db, Bien b)
    {
        b.Reference = txtRef.Text.Trim();
        b.Type = (TypeBien)cmbType.SelectedItem!;
        b.Surface = (double)numSurface.Value;
        b.NbPieces = (int)numPieces.Value;
        b.LoyerBase = numLoyer.Value;
        b.Statut = (StatutBien)cmbStatut.SelectedItem!;

        var adresse = txtAdresse.Text.Trim();
        var ville = txtVille.Text.Trim();
        var loc = db.Localisations.FirstOrDefault(l => l.Adresse == adresse && l.Ville == ville);
        if (loc == null)
            loc = new Localisation { Adresse = adresse, Ville = ville, CodePostal = txtCP.Text.Trim() };
        b.Localisation = loc;
    }

    private bool Valider()
    {
        if (txtRef.Text.Trim() == "") { Erreur("La référence est obligatoire."); return false; }
        if (txtAdresse.Text.Trim() == "" || txtVille.Text.Trim() == "") { Erreur("L'adresse et la ville sont obligatoires."); return false; }
        return true;
    }

    // ---------- Boutons ----------
    private void Ajouter()
    {
        if (!Valider()) return;
        try
        {
            using var db = new AppDbContext();
            var b = new Bien();
            Remplir(db, b);
            db.Biens.Add(b);
            db.SaveChanges();
            Vider();
            Charger();
        }
        catch (DbUpdateException)
        {
            Erreur("Cette référence existe déjà.");
        }
    }

    private void Modifier()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un bien dans le tableau."); return; }
        if (!Valider()) return;
        try
        {
            using var db = new AppDbContext();
            var b = db.Biens.Find(selectedId.Value);
            if (b == null) return;
            Remplir(db, b);
            db.SaveChanges();
            Charger();
        }
        catch (DbUpdateException)
        {
            Erreur("Cette référence existe déjà.");
        }
    }

    private void Supprimer()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un bien dans le tableau."); return; }

        var r = MessageBox.Show(
            "Supprimer ce bien supprimera aussi ses contrats, loyers et maintenances. Continuer ?",
            "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        using var db = new AppDbContext();
        var b = db.Biens.Find(selectedId.Value);
        if (b != null)
        {
            db.Biens.Remove(b);
            db.SaveChanges();
        }
        Vider();
        Charger();
    }

    private void Vider()
    {
        selectedId = null;
        txtRef.Clear(); txtAdresse.Clear(); txtVille.Clear(); txtCP.Clear();
        numSurface.Value = 0; numPieces.Value = 0; numLoyer.Value = 0;
        cmbType.SelectedIndex = 0; cmbStatut.SelectedIndex = 0;
    }
}