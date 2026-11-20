using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;

public class LoyersControl : UserControl
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

    private readonly ComboBox cmbFiltre = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown numPaiement = new() { DecimalPlaces = 2, Maximum = 10000000 };
    private readonly DateTimePicker dtPaiement = new() { Format = DateTimePickerFormat.Short };
    private readonly Label lblTotal = new() { AutoSize = true, Left = 10, Top = 10 };

    private int? selectedId = null;

    public LoyersControl()
    {
        cmbFiltre.Items.AddRange(new object[] { "Tous les loyers", "En retard", "Non payés", "Payés" });
        cmbFiltre.SelectedIndex = 0;
        cmbFiltre.SelectedIndexChanged += (s, e) => Charger();

        // Zone de filtre + total
        var pnlHaut = new Panel { Dock = DockStyle.Top, Height = 70 };
        pnlHaut.Controls.Add(new Label { Text = "Afficher :", Left = 10, Top = 12, AutoSize = true });
        cmbFiltre.Left = 75; cmbFiltre.Top = 8; cmbFiltre.Width = 160;
        pnlHaut.Controls.Add(cmbFiltre);
        lblTotal.Top = 42;
        pnlHaut.Controls.Add(lblTotal);

        // Zone de paiement
        var pnlPaiement = new Panel { Dock = DockStyle.Bottom, Height = 80 };
        Champ(pnlPaiement, "Montant payé (DT)", numPaiement, 10, 5, 150);
        Champ(pnlPaiement, "Date du paiement", dtPaiement, 180, 5, 130);
        var b = new Button { Text = "Enregistrer le paiement", Left = 330, Top = 25, Width = 190, Height = 30 };
        b.Click += (s, e) => Payer();
        pnlPaiement.Controls.Add(b);

        grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                Selectionner(Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value));
        };

        Controls.Add(grid);
        Controls.Add(pnlHaut);
        Controls.Add(pnlPaiement);

        VisibleChanged += (s, e) => { if (Visible) Charger(); };
        Charger();
    }

    private static void Champ(Panel p, string titre, Control c, int x, int y, int largeur)
    {
        p.Controls.Add(new Label { Text = titre, Left = x, Top = y, AutoSize = true });
        c.Left = x; c.Top = y + 20; c.Width = largeur;
        p.Controls.Add(c);
    }

    private static void Erreur(string message)
        => MessageBox.Show(message, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void Charger()
    {
        using var db = new AppDbContext();
        var q = db.Loyers.AsQueryable();
        int courant = DateTime.Today.Year * 12 + DateTime.Today.Month;

        switch (cmbFiltre.SelectedIndex)
        {
            case 1: q = q.Where(l => l.Statut != StatutLoyer.Paye && l.Annee * 12 + l.Mois <= courant); break;
            case 2: q = q.Where(l => l.Statut != StatutLoyer.Paye); break;
            case 3: q = q.Where(l => l.Statut == StatutLoyer.Paye); break;
        }

        var liste = q.OrderBy(l => l.Annee).ThenBy(l => l.Mois)
            .Select(l => new
            {
                l.Id,
                Bien = l.Contrat!.Bien!.Reference,
                Locataire = l.Contrat.Locataire!.Nom + " " + l.Contrat.Locataire.Prenom,
                l.Mois,
                l.Annee,
                l.MontantDu,
                l.MontantPaye,
                l.DatePaiement,
                l.Statut
            })
            .ToList();

        grid.DataSource = liste;
        if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false;
        if (grid.Columns["DatePaiement"] != null) grid.Columns["DatePaiement"]!.DefaultCellStyle.Format = "dd/MM/yyyy";

        // Les sommes sont calculées en mémoire (SQLite ne gère pas bien SUM sur decimal)
        decimal reste = liste.Sum(l => l.MontantDu - l.MontantPaye);
        lblTotal.Text = $"{liste.Count} loyer(s) affiché(s) - reste à payer : {reste:N2} DT";
        selectedId = null;
    }

    private void Selectionner(int id)
    {
        using var db = new AppDbContext();
        var l = db.Loyers.Find(id);
        if (l == null) return;

        selectedId = l.Id;
        // Propose le reste à payer
        numPaiement.Value = Math.Max(0, l.MontantDu - l.MontantPaye);
        dtPaiement.Value = DateTime.Today;
    }

    private void Payer()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur un loyer dans le tableau."); return; }
        if (numPaiement.Value <= 0) { Erreur("Le montant payé doit être supérieur à 0."); return; }

        using var db = new AppDbContext();
        var l = db.Loyers.Find(selectedId.Value);
        if (l == null) return;

        l.MontantPaye += numPaiement.Value;
        l.DatePaiement = dtPaiement.Value.Date;
        l.Statut = l.MontantPaye >= l.MontantDu ? StatutLoyer.Paye : StatutLoyer.Partiel;
        db.SaveChanges();

        Charger();
    }
}