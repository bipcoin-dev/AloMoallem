using AloMoallem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers.Api;

[ApiController]
[Route("api/locations")]
public class LocationsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public LocationsApiController(AppDbContext db) => _db = db;

    [HttpGet("governorates")]
    public async Task<IActionResult> Governorates()
    {
        var list = await _db.Governorates.AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new { g.Id, g.Name })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("neighborhoods")]
    public async Task<IActionResult> Neighborhoods([FromQuery] int governorateId)
    {
        var list = await _db.Neighborhoods.AsNoTracking()
            .Where(n => n.GovernorateId == governorateId)
            .OrderBy(n => n.Name)
            .Select(n => new { n.Id, n.Name })
            .ToListAsync();
        return Ok(list);
    }
}
