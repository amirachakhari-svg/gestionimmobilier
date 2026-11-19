public class Maintenance
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateTime DateDemande { get; set; } = DateTime.Today;
    public DateTime? DateRealisation { get; set; }
    public decimal Cout { get; set; }
    public StatutMaintenance Statut { get; set; } = StatutMaintenance.Demandee;

    public int BienId { get; set; }
    public Bien? Bien { get; set; }
}