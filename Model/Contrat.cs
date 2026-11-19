public class Contrat
{
    public int Id { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal MontantLoyer { get; set; }
    public decimal Caution { get; set; }

    public int BienId { get; set; }
    public Bien? Bien { get; set; }
    public int LocataireId { get; set; }
    public Locataire? Locataire { get; set; }

    public List<Loyer> Loyers { get; set; } = new();
}