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
        onglets.TabPages.Add(CreerOnglet("Biens", new BiensControl()));
        // Les autres onglets seront ajoutés ici, un par un

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