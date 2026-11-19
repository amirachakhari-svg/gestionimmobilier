using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Localisation> Localisations => Set<Localisation>();
    public DbSet<Bien> Biens => Set<Bien>();
    public DbSet<Locataire> Locataires => Set<Locataire>();
    public DbSet<Contrat> Contrats => Set<Contrat>();
    public DbSet<Loyer> Loyers => Set<Loyer>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=immobilier.db");

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Bien>().HasIndex(b => b.Reference).IsUnique();

        // SQLite n'a pas de vrai type decimal : on convertit en double
        mb.Entity<Bien>().Property(b => b.LoyerBase).HasConversion<double>();
        mb.Entity<Contrat>().Property(c => c.MontantLoyer).HasConversion<double>();
        mb.Entity<Contrat>().Property(c => c.Caution).HasConversion<double>();
        mb.Entity<Loyer>().Property(l => l.MontantDu).HasConversion<double>();
        mb.Entity<Loyer>().Property(l => l.MontantPaye).HasConversion<double>();
        mb.Entity<Maintenance>().Property(m => m.Cout).HasConversion<double>();
    }
}