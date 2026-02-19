namespace AloMoallem.Web.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    public string CustomerUserId { get; set; } = "";
    public AppUser CustomerUser { get; set; } = default!;

    public int ProfessionId { get; set; }
    public Profession Profession { get; set; } = default!;

    public int GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = default!;

    public int NeighborhoodId { get; set; }
    public Neighborhood Neighborhood { get; set; } = default!;

    public string Description { get; set; } = "";

    // New -> Accepted -> InProgress -> Completed -> Cancelled
    public string Status { get; set; } = "New";

    // أول حرفي يقبل الطلب
    public string? AssignedArtisanUserId { get; set; }
    public AppUser? AssignedArtisanUser { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<ServiceRequestOffer> Offers { get; set; } = new();
}
