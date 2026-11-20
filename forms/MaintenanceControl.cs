using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;

public class MaintenanceControl : UserControl
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
    private readonly ComboBox cmbStatut = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtDescription = new();
    private readonly DateTimePicker dtDemande = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker dtRealisation = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly NumericUpDown numCout = new() { DecimalPlaces = 2, Maximum = 10000000 };

    private int? selectedId = null;

    public MaintenanceControl()
    {
        cmbBien.DisplayMember = "Texte"; cmbBien.ValueMember = "Id";
        cmbStatut.DataSource = Enum.GetValues<StatutMaintenance>();

        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 180 };
        Champ(pnlForm, "Bien", cmbBien, 10, 10, 220);
        Champ(pnlForm, "Description du problème", txtDescription, 250, 10, 350);
        Champ(pnlForm, "Statut", cmbStatut, 620, 10, 130);
        Champ(pnlForm, "Date de demande", dtDemande, 10, 70, 130);
        Champ(pnlForm, "Date de réalisation", dtRealisation, 160, 70, 140);
        Champ(pnlForm, "Coût (DT)", numCout, 320, 70, 120);

        Bouton(pnlForm, "Ajouter", 10, 135, (s, e) => Ajouter());
        Bouton(pnlForm, "Modifier", 120, 135, (s, e) => Modifier());
        Bouton(pnlForm, "Supprimer", 230, 135, (s, e) => Supprimer());
        Bouton(pnlForm, "Vider", 340, 135, (s, e) => Vider());

        grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                Selectionner(Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value));
        };

        Controls.Add(grid);
        Controls.Add(pnlForm);

        VisibleChanged += (s, e) => { if (Visible) { ChargerListes(); Charger(); } };
        ChargerListes();
        Charger();
    }

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

    private void ChargerListes()
    {
        using var db = new AppDbContext();
        cmbBien.DataSource = db.Biens.Include(b => b.Localisation)
            .OrderBy(b => b.Reference)
            .AsEnumerable()
            .Select(b => new Item(b.Id, $"{b.Reference} - {b.Localisation?.Ville}"))
            .ToList();
    }

    private void Charger()
    {
        using var db = new AppDbContext();
        grid.DataSource = db.Maintenances
            .OrderByDescending(m => m.DateDemande)
            .Select(m => new
            {
                m.Id,
                Bien = m.Bien!.Reference,
                m.Description,
                m.DateDemande,
                m.DateRealisation,
                m.Statut,
                m.Cout
            })
            .ToList();

        if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false;
        if (grid.Columns["DateDemande"] != null) grid.Columns["DateDemande"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
        if (grid.Columns["DateRealisation"] != null) grid.Columns["DateRealisation"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
    }

    private void Selectionner(int id)
    {
        using var db = new AppDbContext();
        var m = db.Maintenances.Find(id);
        if (m == null) return;

        selectedId = m.Id;
        cmbBien.SelectedValue = m.BienId;
        txtDescription.Text = m.Description;
        dtDemande.Value = m.DateDemande;
        cmbStatut.SelectedItem = m.Statut;
        numCout.Value = m.Cout;

        if (m.DateRealisation != null)
        {
            dtRealisation.Checked = true;
            dtRealisation.Value = m.DateRealisation.Value;
        }
        else
        {
            dtRealisation.Checked = false;
        }
    }

    private bool Valider()
    {
        if (cmbBien.SelectedValue == null) { Erreur("Choisis un bien (ajoute-en un dans l'onglet Biens si la liste est vide)."); return false; }
        if (txtDescription.Text.Trim() == "") { Erreur("La description est obligatoire."); return false; }
        return true;
    }

    private void Remplir(Maintenance m)
    {
        m.BienId = Convert.ToInt32(cmbBien.SelectedValue);
        m.Description = txtDescription.Text.Trim();
        m.DateDemande = dtDemande.Value.Date;
        m.DateRealisation = dtRealisation.Checked ? dtRealisation.Value.Date : null;
        m.Statut = (StatutMaintenance)cmbStatut.SelectedItem!;
        m.Cout = numCout.Value;
    }

    private void Ajouter()
    {
        if (!Valider()) return;
        using var db = new AppDbContext();
        var m = new Maintenance();
        Remplir(m);
        db.Maintenances.Add(m);
        db.SaveChanges();
        Vider();
        Charger();
    }

    private void Modifier()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur une intervention dans le tableau."); return; }
        if (!Valider()) return;

        using var db = new AppDbContext();
        var m = db.Maintenances.Find(selectedId.Value);
        if (m == null) return;
        Remplir(m);
        db.SaveChanges();
        Charger();
    }

    private void Supprimer()
    {
        if (selectedId == null) { Erreur("Clique d'abord sur une intervention dans le tableau."); return; }

        var r = MessageBox.Show("Supprimer cette intervention ?", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        using var db = new AppDbContext();
        var m = db.Maintenances.Find(selectedId.Value);
        if (m != null)
        {
            db.Maintenances.Remove(m);
            db.SaveChanges();
        }
        Vider();
        Charger();
    }

    private void Vider()
    {
        selectedId = null;
        txtDescription.Clear();
        dtDemande.Value = DateTime.Today;
        dtRealisation.Checked = false;
        cmbStatut.SelectedIndex = 0;
        numCout.Value = 0;
    }
}