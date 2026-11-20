using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;

public class ContratsControl : UserControl
{
    private record Item(int Id, string Texte);

    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly ComboBox cmbBien = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbLocataire = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtDebut = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker dtFin = new() { Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown numLoyer = new() { DecimalPlaces = 2, Maximum = 10000000 };
    private readonly NumericUpDown numCaution = new() { DecimalPlaces = 2, Maximum = 10000000 };

    private int? selectedId = null;

    public ContratsControl()
    {
        cmbBien.DisplayMember = "Texte"; cmbBien.ValueMember = "Id";
        cmbLocataire.DisplayMember = "Texte"; cmbLocataire.ValueMember = "Id";
        dtFin.Value = dtDebut.Value.AddYears(1);

        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 180 };
        Champ(pnlForm, "Bien", cmbBien, 10, 10, 220);
        Champ(pnlForm, "Locataire", cmbLocataire, 250, 10, 220);
        Champ(pnlForm, "Date de début", dtDebut, 490, 10, 120);
        Champ(pnlForm, "Date de fin", dtFin, 630, 10, 120);
        Champ(pnlForm, "Loyer mensuel (DT)", numLoyer, 10, 70, 140);
        Champ(pnlForm, "Caution (DT)", numCaution, 170, 70, 140);

        Bouton(pnlForm, "Ajouter", 10, 135, 100, (s, e) => Ajouter());
        Bouton(pnlForm, "Modifier", 120, 135, 100, (s, e) => Modifier());
        Bouton(pnlForm, "Supprimer", 230, 135, 100, (s, e) => Supprimer());
        Bouton(pnlForm, "Vider", 340, 135, 100, (s, e) => Vider());
        Bouton(pnlForm, "Générer les loyers", 450, 135, 160, (s, e) => GenererLoyers());

        grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                Selectionner(Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value));
        };

        Controls.Add(grid);
        Controls.Add(pnlForm);

        // Recharge les listes à chaque fois qu'on revient sur cet onglet
        VisibleChanged += (s, e) => { if (Visible) { ChargerListes(); Charger(); } };

        ChargerListes();
        Charger();
    }

    // ---------- Outils d'affichage ----------
    private static void Champ(Panel p, string titre, Control c, int x, int y, int largeur)
    {
        p.Controls.Add(new Label { Text = titre, Left = x, Top = y, AutoSize = true });
        c.Left = x; c.Top = y + 20; c.Width = largeur;
        p.Controls.Add(c);
    }

    private static void Bouton(Panel p, string texte, int x, int y, int largeur, EventHandler action)
    {
        var b = new Button { Text = texte, Left = x, Top = y, Width = largeur, Height = 30 };
        b.Click += action;
        p.Controls.Add(b);
    }

    private static void Erreur(string message)
        => MessageBox.Show(message, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // ---------- Accès à la base (Entity Framework) ----------
    private void ChargerListes()
    {
        using var db = new AppDbContext();

        cmbBien.DataSource = db.Biens.Include(b => b.Localisation)
            .OrderBy(b => b.Reference)
            .AsEnumerable()
            .Select(b => new Item(b.Id, $"{b.Reference} - {b.Localisation?.Ville}"))
            .ToList();

        cmbLocataire.DataSource = db.Locataires
            .OrderBy(l => l.Nom)
            .AsEnumerable()
            .Select(l => new Item(l.Id, $"{l.Nom} {l.Prenom}"))
            .ToList();
    }

    private void Charger()
    {
        using var db = new AppDbContext();
        grid.DataSource = db.Contrats
            .OrderByDescending(c => c.DateDebut)
            .Select(c => new
            {
                c.Id,
                Bien = c.Bien!.Reference,
                Locataire = c.Locataire!.Nom + " " + c.Locataire.Prenom,
                c.DateDebut,
                c.DateFin,
                c.MontantLoyer,
                c.Caution
            })
            .ToList();

        if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false;
        if (grid.Columns["DateDebut"] != null) grid.Columns["DateDebut"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
        if (grid.Columns["DateFin"] != null) grid.Columns["DateFin"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
    }

    private void Selectionner(int id)
    {
        using var db = new AppDbContext();
        var c = db.Contrats.Find(id);
        if (c == null) return;

        selectedId = c.Id;
        cmbBien.SelectedValue = c.BienId;
        cmbLocataire.SelectedValue = c.LocataireId;
        dtDebut.Value = c.DateDebut;
        dtFin.Value = c.DateFin;
        numLoyer.Value = c.MontantLoyer;
        numCaution.Value = c.Caution;
    }

    private bool Valider()
    {
        if (cmbBien.SelectedValue == null) { Erreur("Choisis un bien (ajoute-en un dans l'onglet Biens si la liste est vide)."); return false; }
        if (cmbLocataire.SelectedValue == null) { Erreur("Choisis un locataire (ajoute-en un dans l'onglet Locataires si la liste est vide)."); return false; }
        if (dtFin.Value.Date <= dtDebut.Value.Date) { Erreur("La date de fin doit être après la date de début."); return false; }
        if (numLoyer.Value <= 0) { Erreur("Le loyer mensuel doit être supérieur à 0."); return false; }
        return true;
    }

    private void Remplir(Contrat c)
    {
        c.BienId = Convert.ToInt32(cmbBien.SelectedValue);
        c.LocataireId = Convert.ToInt32(cmbLocataire.SelectedValue);
        c.DateDebut = dtDebut.Value.Date;
        c.DateFin = dtFin.Value.Date;
        c.MontantLoyer = numLoyer.Value;
        c.Caution = numCaution.Value;
    }

    // ---------- Boutons ----------
    private void Ajouter()
    {
        if (!Valider()) return;
        using var db = new AppDbContext();
        var c = new Contrat();
        Remplir(c);
        db.Contrats.Add(c);

        // Le bien devient "Loué"
        var bien = db.Biens.Find(c.BienId);
        if (bien != null) bien.Statut = StatutBien.Loue;

        db.SaveChanges();
        Vider();
        Charger();
    }

    private void Modifier()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un contrat dans le tableau."); return; }
        if (!Valider()) return;

        using var db = new AppDbContext();
        var c = db.Contrats.Find(selectedId.Value);
        if (c == null) return;
        Remplir(c);
        db.SaveChanges();
        Charger();
    }

    private void Supprimer()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un contrat dans le tableau."); return; }

        var r = MessageBox.Show(
            "Supprimer ce contrat supprimera aussi tous ses loyers. Continuer ?",
            "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        using var db = new AppDbContext();
        var c = db.Contrats.Find(selectedId.Value);
        if (c != null)
        {
            db.Contrats.Remove(c);
            db.SaveChanges();
        }
        Vider();
        Charger();
    }

    private void GenererLoyers()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un contrat dans le tableau."); return; }

        using var db = new AppDbContext();
        new LoyerService(db).GenererLoyers(selectedId.Value);
        MessageBox.Show("Les loyers mensuels ont été générés pour ce contrat.", "Information",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Vider()
    {
        selectedId = null;
        dtDebut.Value = DateTime.Today;
        dtFin.Value = DateTime.Today.AddYears(1);
        numLoyer.Value = 0;
        numCaution.Value = 0;
    }
}