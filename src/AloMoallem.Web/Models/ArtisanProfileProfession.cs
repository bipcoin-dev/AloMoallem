namespace AloMoallem.Web.Models;

// Many-to-many: artisan يمكن يختار أكثر من مهنة
public class ArtisanProfileProfession
{
    public int ArtisanProfileId { get; set; }
    public ArtisanProfile ArtisanProfile { get; set; } = default!;

    public int ProfessionId { get; set; }
    public Profession Profession { get; set; } = default!;

    public DateTime LinkedAtUtc { get; set; } = DateTime.UtcNow;
}
