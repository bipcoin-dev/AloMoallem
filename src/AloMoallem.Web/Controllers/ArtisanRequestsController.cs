using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

[Authorize(Roles = "Artisan")]
public class ArtisanRequestsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly AloMoallem.Web.Services.NotificationService _notify;

    public ArtisanRequestsController(AppDbContext db, UserManager<AppUser> userManager, AloMoallem.Web.Services.NotificationService notify)
    {
        _db = db;
        _userManager = userManager;
        _notify = notify;
    }

    public async Task<IActionResult> Inbox()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var offers = await _db.ServiceRequestOffers
            .Include(o => o.ServiceRequest)
                .ThenInclude(r => r.Profession)
            .Include(o => o.ServiceRequest)
                .ThenInclude(r => r.Governorate)
            .Include(o => o.ServiceRequest)
                .ThenInclude(r => r.Neighborhood)
            .AsNoTracking()
            .Where(o => o.ArtisanUserId == me.Id && o.Status == "Pending")
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(50)
            .ToListAsync();

        return View(offers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int offerId)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        // Transaction: أول واحد يقبل ياخده
        await using var tx = await _db.Database.BeginTransactionAsync();

        var offer = await _db.ServiceRequestOffers
            .Include(o => o.ServiceRequest)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer is null) return NotFound();
        if (offer.ArtisanUserId != me.Id) return Forbid();
        if (offer.Status != "Pending") return RedirectToAction(nameof(Inbox));

        var req = offer.ServiceRequest;
        if (req.Status != "New" || req.AssignedArtisanUserId != null)
        {
            // صار مأخوذ
            offer.Status = "Expired";
            offer.RespondedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return RedirectToAction(nameof(Inbox));
        }

        // Assign
        req.AssignedArtisanUserId = me.Id;
        req.Status = "Accepted";

        offer.Status = "Accepted";
        offer.RespondedAtUtc = DateTime.UtcNow;

        // Expire others
        var others = await _db.ServiceRequestOffers
            .Where(o => o.ServiceRequestId == req.Id && o.Id != offer.Id && o.Status == "Pending")
            .ToListAsync();

        foreach (var o in others)
        {
            o.Status = "Expired";
            o.RespondedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // إشعار للزبون
        await _notify.NotifyAsync(req.CustomerUserId, "تم قبول طلبك", "حرفي قبل طلبك — افتح طلباتك أو الدردشة", "/Requests/My");

        return RedirectToAction(nameof(Inbox));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int offerId)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var offer = await _db.ServiceRequestOffers.FirstOrDefaultAsync(o => o.Id == offerId);
        if (offer is null) return NotFound();
        if (offer.ArtisanUserId != me.Id) return Forbid();

        if (offer.Status == "Pending")
        {
            offer.Status = "Rejected";
            offer.RespondedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Inbox));
    }
}
