using Microsoft.AspNetCore.Identity;

namespace AloMoallem.Web.Models;

public class AppUser : IdentityUser
{
    // Customer أو Artisan أو Admin
    public string AccountType { get; set; } = "Customer";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ArtisanProfile? ArtisanProfile { get; set; }
    public CustomerProfile? CustomerProfile { get; set; }
}
