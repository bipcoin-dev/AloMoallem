using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<ArtisanProfile> ArtisanProfiles => Set<ArtisanProfile>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<WorkPhoto> WorkPhotos => Set<WorkPhoto>();
    public DbSet<ArtisanProfileProfession> ArtisanProfileProfessions => Set<ArtisanProfileProfession>();

    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceRequestOffer> ServiceRequestOffers => Set<ServiceRequestOffer>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
    .HasOne(u => u.CustomerProfile)
    .WithOne(p => p.User)
    .HasForeignKey<CustomerProfile>(p => p.UserId)
    .OnDelete(DeleteBehavior.Cascade);

builder.Entity<Governorate>()
    .HasMany(g => g.Neighborhoods)
    .WithOne(n => n.Governorate)
    .HasForeignKey(n => n.GovernorateId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AppUser>()
            .HasOne(u => u.ArtisanProfile)
            .WithOne(p => p.User)
            .HasForeignKey<ArtisanProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Conversation>()
            .HasIndex(c => new { c.CustomerUserId, c.ArtisanUserId })
            .IsUnique();

        builder.Entity<Message>()
            .Property(m => m.Text)
            .HasMaxLength(2000);

        // Many-to-many: ArtisanProfile <-> Profession
        builder.Entity<ArtisanProfileProfession>()
            .HasKey(x => new { x.ArtisanProfileId, x.ProfessionId });

        builder.Entity<ArtisanProfileProfession>()
            .HasOne(x => x.ArtisanProfile)
            .WithMany(a => a.ProfessionLinks)
            .HasForeignKey(x => x.ArtisanProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ArtisanProfileProfession>()
            .HasOne(x => x.Profession)
            .WithMany()
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
