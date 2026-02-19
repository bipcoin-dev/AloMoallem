using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public AdminDashboardController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.CountAsync();
        var artisans = await _db.ArtisanProfiles.CountAsync();
        var professions = await _db.Professions.CountAsync();
        var conversations = await _db.Conversations.CountAsync();
        var messages = await _db.Messages.CountAsync();

        var pending = await _db.ArtisanProfiles
            .Include(a => a.Profession)
            .Include(a => a.User)
            .Include(a => a.Governorate)
            .Include(a => a.Neighborhood)
            .Where(a => !a.IsVerified)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(20)
            .ToListAsync();

        var vm = new AdminDashboardVm
        {
            TotalUsers = users,
            TotalArtisans = artisans,
            TotalProfessions = professions,
            TotalConversations = conversations,
            TotalMessages = messages,
            PendingVerification = pending
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(int artisanProfileId, bool isVerified)
    {
        var ap = await _db.ArtisanProfiles.FirstOrDefaultAsync(a => a.Id == artisanProfileId);
        if (ap is null) return NotFound();

        ap.IsVerified = isVerified;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

public class AdminDashboardVm
{
    public int TotalUsers { get; set; }
    public int TotalArtisans { get; set; }
    public int TotalProfessions { get; set; }
    public int TotalConversations { get; set; }
    public int TotalMessages { get; set; }

    public List<ArtisanProfile> PendingVerification { get; set; } = new();
}
