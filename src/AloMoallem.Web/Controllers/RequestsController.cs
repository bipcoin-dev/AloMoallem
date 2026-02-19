using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AloMoallem.Web.Controllers;

[Authorize(Roles = "Customer")]
public class RequestsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly AloMoallem.Web.Services.NotificationService _notify;

    public RequestsController(AppDbContext db, UserManager<AppUser> userManager, AloMoallem.Web.Services.NotificationService notify)
    {
        _db = db;
        _userManager = userManager;
        _notify = notify;
    }

    public async Task<IActionResult> My()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var list = await _db.ServiceRequests
            .Include(r => r.Profession)
            .Include(r => r.Governorate)
            .Include(r => r.Neighborhood)
            .Include(r => r.AssignedArtisanUser)
            .AsNoTracking()
            .Where(r => r.CustomerUserId == me.Id)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(50)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? professionId)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profs = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        ViewBag.Professions = profs;

        var profile = await _db.CustomerProfiles
            .Include(p => p.Governorate)
            .Include(p => p.Neighborhood)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == me.Id);

        var vm = new CreateRequestVm();
        if (professionId.HasValue) vm.ProfessionId = professionId.Value;

        if (profile is not null)
        {
            vm.GovernorateId = profile.GovernorateId;
            vm.NeighborhoodId = profile.NeighborhoodId;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestVm vm)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profs = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        ViewBag.Professions = profs;

        if (!ModelState.IsValid) return View(vm);

        // Create request
        var req = new ServiceRequest
        {
            CustomerUserId = me.Id,
            ProfessionId = vm.ProfessionId,
            GovernorateId = vm.GovernorateId,
            NeighborhoodId = vm.NeighborhoodId,
            Description = vm.Description.Trim(),
            Status = "New",
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.ServiceRequests.Add(req);
        await _db.SaveChangesAsync();

        // Broadcast offers to all artisans with same profession + same area
        var artisanUserIds = await _db.ArtisanProfiles.AsNoTracking()
            .Include(a => a.ProfessionLinks)
            .Where(a => (a.ProfessionId == vm.ProfessionId || a.ProfessionLinks.Any(x => x.ProfessionId == vm.ProfessionId))
                && a.GovernorateId == vm.GovernorateId
                && a.NeighborhoodId == vm.NeighborhoodId)
            .Select(a => a.UserId)
            .ToListAsync();

        foreach (var artisanId in artisanUserIds)
        {
            _db.ServiceRequestOffers.Add(new ServiceRequestOffer
            {
                ServiceRequestId = req.Id,
                ArtisanUserId = artisanId,
                Status = "Pending",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        // Realtime notifications للحرفيين
        foreach (var artisanId in artisanUserIds)
        {
            await _notify.NotifyAsync(
                artisanId,
                "طلب جديد ضمن منطقتك",
                $"طلب {profs.FirstOrDefault(p=>p.Id==vm.ProfessionId)?.Name ?? "خدمة"} جديد — افتح صندوق الطلبات",
                "/ArtisanRequests/Inbox");
        }

        return RedirectToAction(nameof(My));
    }
}

public class CreateRequestVm
{
    [Required]
    public int ProfessionId { get; set; }

    [Required]
    public int GovernorateId { get; set; }

    [Required]
    public int NeighborhoodId { get; set; }

    [Required, MinLength(10)]
    public string Description { get; set; } = "";
}
