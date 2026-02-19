namespace AloMoallem.Web.Models;

public class Profession
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "🛠️";
    public string Description { get; set; } = "";

    public List<ArtisanProfile> Artisans { get; set; } = new();
}
