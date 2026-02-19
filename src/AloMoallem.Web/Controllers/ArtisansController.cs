using AloMoallem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

public class ArtisansController : Controller
{
    private readonly AppDbContext _db;
    public ArtisansController(AppDbContext db) => _db = db;

    [HttpGet("/a/{id:int}")]
    public async Task<IActionResult> Profile(int id)
    {
        var artisan = await _db.ArtisanProfiles
            .Include(a => a.Profession)
            .Include(a => a.ProfessionLinks)
            .ThenInclude(x => x.Profession)
            .Include(a => a.User)
            .Include(a => a.Governorate)
            .Include(a => a.Neighborhood)
            .Include(a => a.WorkPhotos.OrderByDescending(w => w.CreatedAtUtc))
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artisan is null) return NotFound();
        return View(artisan);
    }
}
