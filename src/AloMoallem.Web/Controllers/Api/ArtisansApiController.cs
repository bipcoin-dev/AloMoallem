using AloMoallem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers.Api;

[ApiController]
[Route("api/artisans")]
public class ArtisansApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ArtisansApiController(AppDbContext db) => _db = db;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var a = await _db.ArtisanProfiles
            .Include(x => x.Profession)
            .Include(x => x.ProfessionLinks)
            .ThenInclude(x => x.Profession)
            .Include(x => x.Governorate)
            .Include(x => x.Neighborhood)
            .Include(x => x.WorkPhotos.OrderByDescending(w => w.CreatedAtUtc))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (a is null) return NotFound();

        return Ok(new {
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
            Neighborhood = a.Neighborhood.Name,
            a.ServiceCallFee,
            a.HourlyRate,
            professions = a.ProfessionLinks.Select(p => new { p.ProfessionId, name = p.Profession.Name, icon = p.Profession.Icon }).Distinct(),
            primaryProfession = new { a.Profession.Id, a.Profession.Name, a.Profession.Icon },
            workPhotos = a.WorkPhotos.Select(w => new { w.Id, w.Url })
        });
    }
}
