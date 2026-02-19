using AloMoallem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

public class ProfessionsController : Controller
{
    private readonly AppDbContext _db;
    public ProfessionsController(AppDbContext db) => _db = db;

    [HttpGet("/p/{id:int}", Name="ProfessionPage")]
    public async Task<IActionResult> Details(int id)
    {
        var profession = await _db.Professions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (profession is null) return NotFound();

        // حرفيين مرتبطين بهذه المهنة (أساسية أو ضمن المهن الإضافية)
        var artisans = await _db.ArtisanProfiles
            .Include(a => a.User)
            .Include(a => a.Governorate)
            .Include(a => a.Neighborhood)
            .Include(a => a.ProfessionLinks)
            .ThenInclude(x => x.Profession)
            .AsNoTracking()
            .Where(a => a.ProfessionId == id || a.ProfessionLinks.Any(x => x.ProfessionId == id))
            .OrderByDescending(a => a.AvailableNow)
            .ThenByDescending(a => a.Rating)
            .ToListAsync();

        return View(new ProfessionDetailsVm { Profession = profession, Artisans = artisans });
    }
}

public class ProfessionDetailsVm
{
    public AloMoallem.Web.Models.Profession Profession { get; set; } = default!;
    public List<AloMoallem.Web.Models.ArtisanProfile> Artisans { get; set; } = new();
}
