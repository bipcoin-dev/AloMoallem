namespace AloMoallem.Web.Models;

public class ServiceRequestOffer
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = default!;

    public string ArtisanUserId { get; set; } = "";
    public AppUser ArtisanUser { get; set; } = default!;

    // Pending -> Accepted -> Rejected -> Expired
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAtUtc { get; set; }
}
