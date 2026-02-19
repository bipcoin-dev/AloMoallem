namespace AloMoallem.Web.Models;

public class WorkPhoto
{
    public int Id { get; set; }

    public int ArtisanProfileId { get; set; }
    public ArtisanProfile ArtisanProfile { get; set; } = default!;

    public string Url { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
