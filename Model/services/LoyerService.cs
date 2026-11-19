using Microsoft.EntityFrameworkCore;

public class LoyerService
{
    private readonly AppDbContext _db;
    public LoyerService(AppDbContext db) => _db = db;

    // Génère un loyer par mois pour la durée du contrat
    public void GenererLoyers(int contratId)
    {
        var c = _db.Contrats.Find(contratId) ?? throw new Exception("Contrat introuvable");
        var d = new DateTime(c.DateDebut.Year, c.DateDebut.Month, 1);

        while (d <= c.DateFin)
        {
            bool existe = _db.Loyers.Any(l => l.ContratId == c.Id && l.Mois == d.Month && l.Annee == d.Year);
            if (!existe)
                _db.Loyers.Add(new Loyer { ContratId = c.Id, Mois = d.Month, Annee = d.Year, MontantDu = c.MontantLoyer });
            d = d.AddMonths(1);
        }
        _db.SaveChanges();
    }

    // Liste des loyers non payés, avec le bien et le locataire
    public List<Loyer> Impayes() => _db.Loyers
        .Include(l => l.Contrat!).ThenInclude(c => c.Bien)
        .Include(l => l.Contrat!).ThenInclude(c => c.Locataire)
        .Where(l => l.Statut != StatutLoyer.Paye)
        .ToList();
}