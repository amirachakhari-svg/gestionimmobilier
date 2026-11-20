using System.Windows.Forms;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Gestion des biens immobiliers";
        Width = 1150;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        var onglets = new TabControl { Dock = DockStyle.Fill };
        onglets.TabPages.Add(CreerOnglet("Tableau de bord", new DashboardControl()));
        onglets.TabPages.Add(CreerOnglet("Biens", new BiensControl()));
        onglets.TabPages.Add(CreerOnglet("Locataires", new LocatairesControl()));
        onglets.TabPages.Add(CreerOnglet("Contrats", new ContratsControl()));
        onglets.TabPages.Add(CreerOnglet("Loyers", new LoyersControl()));
        onglets.TabPages.Add(CreerOnglet("Maintenance", new MaintenanceControl()));

        Controls.Add(onglets);
    }

    private static TabPage CreerOnglet(string titre, Control contenu)
    {
        contenu.Dock = DockStyle.Fill;
        var page = new TabPage(titre);
        page.Controls.Add(contenu);
        return page;
    }
}