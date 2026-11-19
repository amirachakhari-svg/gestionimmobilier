public class Loyer
{
    public int Id { get; set; }
    public int Mois { get; set; }
    public int Annee { get; set; }
    public decimal MontantDu { get; set; }
    public decimal MontantPaye { get; set; }
    public DateTime? DatePaiement { get; set; }
    public StatutLoyer Statut { get; set; } = StatutLoyer.Impaye;

    public int ContratId { get; set; }
    public Contrat? Contrat { get; set; }
}