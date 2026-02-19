namespace AloMoallem.Web.Models;

public class ArtisanProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = default!;

    public int ProfessionId { get; set; }
    public Profession Profession { get; set; } = default!;

    // مهن إضافية (بما فيها المهنة الأساسية أيضاً، نضيفها تلقائياً)
    public List<ArtisanProfileProfession> ProfessionLinks { get; set; } = new();

    public string DisplayName { get; set; } = "";
    public int GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = default!;

    public int NeighborhoodId { get; set; }
    public Neighborhood Neighborhood { get; set; } = default!;

    public string City { get; set; } = "";
    public string About { get; set; } = "";
    public string PhoneNumberPublic { get; set; } = "";
    public string PhotoUrl { get; set; } = "/img/default-avatar.svg";

    public bool IsVerified { get; set; } = false;

    public bool AvailableNow { get; set; } = true;
    public double Rating { get; set; } = 4.8;
    public int CompletedJobs { get; set; } = 24;

    // تسعير بسيط (نسخة تجريبية)
    public decimal ServiceCallFee { get; set; } = 0;
    public decimal HourlyRate { get; set; } = 0;

    public List<WorkPhoto> WorkPhotos { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
