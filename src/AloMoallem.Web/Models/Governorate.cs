namespace AloMoallem.Web.Models;

public class Governorate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public List<Neighborhood> Neighborhoods { get; set; } = new();
}
