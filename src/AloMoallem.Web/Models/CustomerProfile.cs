namespace AloMoallem.Web.Models;

public class CustomerProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = default!;

    public string FullName { get; set; } = "";
    public int GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = default!;

    public int NeighborhoodId { get; set; }
    public Neighborhood Neighborhood { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
