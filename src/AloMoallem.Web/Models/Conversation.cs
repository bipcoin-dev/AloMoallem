namespace AloMoallem.Web.Models;

public class Conversation
{
    public int Id { get; set; }

    public string CustomerUserId { get; set; } = "";
    public AppUser CustomerUser { get; set; } = default!;

    public string ArtisanUserId { get; set; } = "";
    public AppUser ArtisanUser { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Used for sorting conversations by latest activity.
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Message> Messages { get; set; } = new();
}
