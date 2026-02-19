using AloMoallem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers.Api;

[ApiController]
[Route("api/professions")]
public class ProfessionsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProfessionsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Professions.AsNoTracking()
            .Select(p => new { p.Id, p.Name, p.Icon, p.Description })
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}/artisans")]
    public async Task<IActionResult> GetArtisans(int id)
    {
        var prof = await _db.Professions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (prof is null) return NotFound();

        var artisans = await _db.ArtisanProfiles.AsNoTracking()
            .Include(a=>a.Governorate).Include(a=>a.Neighborhood)
            .Include(a => a.ProfessionLinks)
            .Where(a => a.ProfessionId == id || a.ProfessionLinks.Any(x => x.ProfessionId == id))
            .Select(a => new {
                a.Id,
                a.DisplayName,
                a.City,
                a.About,
                a.PhoneNumberPublic,
                a.PhotoUrl,
                a.IsVerified,
                a.Rating,
                a.CompletedJobs,
                Governorate = a.Governorate.Name,
                Neighborhood = a.Neighborhood.Name
            })
            .OrderByDescending(a => a.IsVerified)
            .ThenByDescending(a => a.Rating)
            .ToListAsync();

        return Ok(new { profession = new { prof.Id, prof.Name, prof.Icon, prof.Description }, artisans });
    }
}
