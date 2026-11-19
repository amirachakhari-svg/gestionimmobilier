using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;

public class LocatairesControl : UserControl
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

    private readonly TextBox txtNom = new(), txtPrenom = new(), txtTel = new(), txtEmail = new(), txtRecherche = new();
    private int? selectedId = null;

    public LocatairesControl()
    {
        // Zone de recherche
        var pnlRecherche = new Panel { Dock = DockStyle.Top, Height = 40 };
        pnlRecherche.Controls.Add(new Label { Text = "Rechercher (nom, prénom ou téléphone) :", Left = 10, Top = 12, AutoSize = true });
        txtRecherche.Left = 290; txtRecherche.Top = 8; txtRecherche.Width = 250;
        txtRecherche.TextChanged += (s, e) => Charger();
        pnlRecherche.Controls.Add(txtRecherche);

        // Zone du formulaire
        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 130 };
        Champ(pnlForm, "Nom", txtNom, 10, 10, 180);
        Champ(pnlForm, "Prénom", txtPrenom, 210, 10, 180);
        Champ(pnlForm, "Téléphone", txtTel, 410, 10, 150);
        Champ(pnlForm, "Email", txtEmail, 580, 10, 220);

        Bouton(pnlForm, "Ajouter", 10, 80, (s, e) => Ajouter());
        Bouton(pnlForm, "Modifier", 120, 80, (s, e) => Modifier());
        Bouton(pnlForm, "Supprimer", 230, 80, (s, e) => Supprimer());
        Bouton(pnlForm, "Vider", 340, 80, (s, e) => Vider());

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
        var q = db.Locataires.AsQueryable();

        var t = txtRecherche.Text.Trim();
        if (t != "")
            q = q.Where(l => l.Nom.Contains(t) || l.Prenom.Contains(t) || l.Telephone.Contains(t));

        grid.DataSource = q.OrderBy(l => l.Nom).ThenBy(l => l.Prenom)
            .Select(l => new { l.Id, l.Nom, l.Prenom, l.Telephone, l.Email })
            .ToList();

        if (grid.Columns["Id"] != null)
            grid.Columns["Id"]!.Visible = false;
    }

    private void Selectionner(int id)
    {
        using var db = new AppDbContext();
        var l = db.Locataires.Find(id);
        if (l == null) return;

        selectedId = l.Id;
        txtNom.Text = l.Nom;
        txtPrenom.Text = l.Prenom;
        txtTel.Text = l.Telephone;
        txtEmail.Text = l.Email;
    }

    private bool Valider()
    {
        if (txtNom.Text.Trim() == "" || txtPrenom.Text.Trim() == "")
        {
            Erreur("Le nom et le prénom sont obligatoires.");
            return false;
        }
        return true;
    }

    private void Remplir(Locataire l)
    {
        l.Nom = txtNom.Text.Trim();
        l.Prenom = txtPrenom.Text.Trim();
        l.Telephone = txtTel.Text.Trim();
        l.Email = txtEmail.Text.Trim();
    }

    // ---------- Boutons ----------
    private void Ajouter()
    {
        if (!Valider()) return;
        using var db = new AppDbContext();
        var l = new Locataire();
        Remplir(l);
        db.Locataires.Add(l);
        db.SaveChanges();
        Vider();
        Charger();
    }

    private void Modifier()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un locataire dans le tableau."); return; }
        if (!Valider()) return;

        using var db = new AppDbContext();
        var l = db.Locataires.Find(selectedId.Value);
        if (l == null) return;
        Remplir(l);
        db.SaveChanges();
        Charger();
    }

    private void Supprimer()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un locataire dans le tableau."); return; }

        var r = MessageBox.Show(
            "Supprimer ce locataire supprimera aussi ses contrats et leurs loyers. Continuer ?",
            "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        using var db = new AppDbContext();
        var l = db.Locataires.Find(selectedId.Value);
        if (l != null)
        {
            db.Locataires.Remove(l);
            db.SaveChanges();
        }
        Vider();
        Charger();
    }

    private void Vider()
    {
        selectedId = null;
        txtNom.Clear(); txtPrenom.Clear(); txtTel.Clear(); txtEmail.Clear();
    }
}