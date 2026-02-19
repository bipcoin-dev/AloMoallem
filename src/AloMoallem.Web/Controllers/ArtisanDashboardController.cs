using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

[Authorize(Roles = "Artisan")]
public class ArtisanDashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ArtisanDashboardController(AppDbContext db, UserManager<AppUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profile = await _db.ArtisanProfiles
            .Include(a => a.Profession)
            .Include(a => a.Governorate)
            .Include(a => a.Neighborhood)
            .Include(a => a.WorkPhotos.OrderByDescending(w => w.CreatedAtUtc))
            .FirstOrDefaultAsync(a => a.UserId == me.Id);

        if (profile is null) return RedirectToAction("Index", "Home");

        var convoCount = await _db.Conversations.CountAsync(c => c.ArtisanUserId == me.Id);
        var pendingOffers = await _db.ServiceRequestOffers.CountAsync(o => o.ArtisanUserId == me.Id && o.Status == "Pending");
        var msgCount = await _db.Messages.CountAsync(m => m.Conversation.ArtisanUserId == me.Id);

        var vm = new ArtisanDashboardVm
        {
            Profile = profile,
            Conversations = convoCount,
            Messages = msgCount,
            PendingOffers = pendingOffers
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profile = await _db.ArtisanProfiles.Include(a => a.Profession)
            .Include(a => a.ProfessionLinks)
            .Include(a => a.Governorate)
            .Include(a => a.Neighborhood).FirstOrDefaultAsync(a => a.UserId == me.Id);
        if (profile is null) return RedirectToAction(nameof(Index));

        var professions = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        ViewBag.Professions = professions;

        var vm = new EditArtisanProfileVm
        {
            DisplayName = profile.DisplayName,
            City = profile.City,
            About = profile.About,
            PhoneNumberPublic = profile.PhoneNumberPublic,
            ProfessionIds = profile.ProfessionLinks.Select(x => x.ProfessionId).DefaultIfEmpty(profile.ProfessionId).Distinct().ToList(),
            ServiceCallFee = profile.ServiceCallFee,
            HourlyRate = profile.HourlyRate
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditArtisanProfileVm vm, IFormFile? avatar)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profile = await _db.ArtisanProfiles
            .Include(a => a.ProfessionLinks)
            .FirstOrDefaultAsync(a => a.UserId == me.Id);
        if (profile is null) return RedirectToAction(nameof(Index));

        if (vm.ProfessionIds is null || vm.ProfessionIds.Count == 0)
            ModelState.AddModelError(nameof(vm.ProfessionIds), "اختر مهنة واحدة على الأقل");

        if (!ModelState.IsValid)
        {
            var professions = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            ViewBag.Professions = professions;
            return View(vm);
        }

        profile.DisplayName = vm.DisplayName;
        profile.City = vm.City;
        profile.About = vm.About;
        profile.PhoneNumberPublic = vm.PhoneNumberPublic;

        var ids = (vm.ProfessionIds ?? new()).Distinct().ToList();
        profile.ProfessionId = ids.First();
        profile.ServiceCallFee = vm.ServiceCallFee;
        profile.HourlyRate = vm.HourlyRate;

        // تحديث روابط المهن
        profile.ProfessionLinks.Clear();
        foreach (var id in ids)
        {
            profile.ProfessionLinks.Add(new ArtisanProfileProfession
            {
                ProfessionId = id,
                ArtisanProfileId = profile.Id
            });
        }

        if (avatar is not null && avatar.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploads);

            var ext = Path.GetExtension(avatar.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

            var fileName = $"{me.Id}{ext}".Replace(":", "_");
            var path = Path.Combine(uploads, fileName);

            using var fs = System.IO.File.Create(path);
            await avatar.CopyToAsync(fs);

            profile.PhotoUrl = $"/uploads/avatars/{fileName}";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadWorkPhoto(IFormFile photo)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profile = await _db.ArtisanProfiles.FirstOrDefaultAsync(a => a.UserId == me.Id);
        if (profile is null) return RedirectToAction(nameof(Index));

        if (photo is null || photo.Length == 0) return RedirectToAction(nameof(Index));

        var uploads = Path.Combine(_env.WebRootPath, "uploads", "works");
        Directory.CreateDirectory(uploads);

        var ext = Path.GetExtension(photo.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var fileName = $"{me.Id}-{Guid.NewGuid():N}{ext}".Replace(":", "_");
        var path = Path.Combine(uploads, fileName);

        using var fs = System.IO.File.Create(path);
        await photo.CopyToAsync(fs);

        _db.WorkPhotos.Add(new WorkPhoto
        {
            ArtisanProfileId = profile.Id,
            Url = $"/uploads/works/{fileName}"
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var profile = await _db.ArtisanProfiles.FirstOrDefaultAsync(a => a.UserId == me.Id);
        if (profile is null) return RedirectToAction(nameof(Index));

        profile.AvailableNow = !profile.AvailableNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}

public class ArtisanDashboardVm
{
    public ArtisanProfile Profile { get; set; } = default!;
    public int Conversations { get; set; }
    public int Messages { get; set; }
    public int PendingOffers { get; set; }

    // Convenience properties used by the view
    public int ProfileId => Profile?.Id ?? 0;
    public bool AvailableNow => Profile?.AvailableNow ?? false;
    public double Rating => Profile?.Rating ?? 0;
    public int CompletedJobs => Profile?.CompletedJobs ?? 0;
}

public class EditArtisanProfileVm
{
    public string DisplayName { get; set; } = "";
    public string City { get; set; } = "";
    public string About { get; set; } = "";
    public string PhoneNumberPublic { get; set; } = "";
    public List<int> ProfessionIds { get; set; } = new();

    public decimal ServiceCallFee { get; set; }
    public decimal HourlyRate { get; set; }
}
