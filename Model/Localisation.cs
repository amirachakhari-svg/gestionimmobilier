public class Localisation
{
    public int Id { get; set; }
    public string Adresse { get; set; } = "";
    public string Ville { get; set; } = "";
    public string CodePostal { get; set; } = "";
    public List<Bien> Biens { get; set; } = new();
}