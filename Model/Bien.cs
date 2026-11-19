public class Bien
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public TypeBien Type { get; set; }
    public double Surface { get; set; }
    public int NbPieces { get; set; }
    public decimal LoyerBase { get; set; }
    public StatutBien Statut { get; set; } = StatutBien.Libre;

    public int LocalisationId { get; set; }
    public Localisation? Localisation { get; set; }

    public List<Contrat> Contrats { get; set; } = new();
    public List<Maintenance> Maintenances { get; set; } = new();
}