using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Windows.Forms;

public class DashboardControl : UserControl
{
    private readonly Label lblBiens = new(), lblOccupation = new(), lblEncaisse = new(),
                           lblRetard = new(), lblMaintenance = new(), lblInterventions = new();

    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public DashboardControl()
    {
        var cartes = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(10) };
        cartes.Controls.Add(Carte("Nombre de biens", lblBiens));
        cartes.Controls.Add(Carte("Taux d'occupation", lblOccupation));
        cartes.Controls.Add(Carte("Loyers encaissés (DT)", lblEncaisse));
        cartes.Controls.Add(Carte("Loyers en retard (DT)", lblRetard));
        cartes.Controls.Add(Carte("Coût maintenance (DT)", lblMaintenance));
        cartes.Controls.Add(Carte("Interventions en cours", lblInterventions));

        var titre = new Label
        {
            Text = "Contrats qui se terminent dans les 60 prochains jours",
            Dock = DockStyle.Top, Height = 30, Padding = new Padding(10, 8, 0, 0),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        Controls.Add(grid);
        Controls.Add(titre);
        Controls.Add(cartes);

        // Se met à jour à chaque fois qu'on ouvre l'onglet
        VisibleChanged += (s, e) => { if (Visible) Actualiser(); };
        Actualiser();
    }

    private static Panel Carte(string titre, Label valeur)
    {
        var p = new Panel { Width = 175, Height = 100, Margin = new Padding(8), BorderStyle = BorderStyle.FixedSingle };
        p.Controls.Add(new Label { Text = titre, Left = 8, Top = 8, AutoSize = true });
        valeur.Left = 8; valeur.Top = 40; valeur.AutoSize = true;
        valeur.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        p.Controls.Add(valeur);
        return p;
    }

    private void Actualiser()
    {
        using var db = new AppDbContext();

        int nbBiens = db.Biens.Count();
        int nbLoues = db.Biens.Count(b => b.Statut == StatutBien.Loue);
        double taux = nbBiens == 0 ? 0 : 100.0 * nbLoues / nbBiens;

        // Sommes calculées en mémoire (SQLite ne gère pas bien SUM sur decimal)
        var loyers = db.Loyers.ToList();
        int courant = DateTime.Today.Year * 12 + DateTime.Today.Month;
        decimal encaisse = loyers.Sum(l => l.MontantPaye);
        decimal retard = loyers
            .Where(l => l.Statut != StatutLoyer.Paye && l.Annee * 12 + l.Mois <= courant)
            .Sum(l => l.MontantDu - l.MontantPaye);

        var maintenances = db.Maintenances.ToList();
        decimal coutMaintenance = maintenances.Sum(m => m.Cout);
        int enCours = maintenances.Count(m => m.Statut != StatutMaintenance.Terminee);

        lblBiens.Text = nbBiens.ToString();
        lblOccupation.Text = $"{taux:F0} %";
        lblEncaisse.Text = encaisse.ToString("N0");
        lblRetard.Text = retard.ToString("N0");
        lblMaintenance.Text = coutMaintenance.ToString("N0");
        lblInterventions.Text = enCours.ToString();

        var aujourdhui = DateTime.Today;
        var limite = aujourdhui.AddDays(60);
        grid.DataSource = db.Contrats
            .Where(c => c.DateFin >= aujourdhui && c.DateFin <= limite)
            .OrderBy(c => c.DateFin)
            .Select(c => new
            {
                Bien = c.Bien!.Reference,
                Locataire = c.Locataire!.Nom + " " + c.Locataire.Prenom,
                c.DateFin,
                c.MontantLoyer
            })
            .ToList();

        if (grid.Columns["DateFin"] != null) grid.Columns["DateFin"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
    }
}