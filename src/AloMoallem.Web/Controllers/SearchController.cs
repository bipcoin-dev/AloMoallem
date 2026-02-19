using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

public class SearchController : Controller
{
    private readonly AppDbContext _db;
    public SearchController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Results([FromQuery] SearchVm vm)
    {
        var q = (vm.Q ?? "").Trim();

        var query = _db.ArtisanProfiles
            .Include(a => a.User)
            .Include(a => a.Profession)
            .Include(a => a.ProfessionLinks)
            .ThenInclude(x => x.Profession)
            .Include(a => a.Governorate)
            .Include(a => a.Neighborhood)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(a =>
                a.DisplayName.Contains(q) ||
                a.About.Contains(q) ||
                a.Profession.Name.Contains(q) ||
                a.ProfessionLinks.Any(x => x.Profession.Name.Contains(q)));
        }

        if (vm.GovernorateId.HasValue && vm.GovernorateId.Value > 0)
            query = query.Where(a => a.GovernorateId == vm.GovernorateId.Value);

        if (vm.NeighborhoodId.HasValue && vm.NeighborhoodId.Value > 0)
            query = query.Where(a => a.NeighborhoodId == vm.NeighborhoodId.Value);

        if (vm.AvailableNow == true)
            query = query.Where(a => a.AvailableNow);

        if (vm.MinRating.HasValue)
            query = query.Where(a => a.Rating >= vm.MinRating.Value);

        query = vm.Sort switch
        {
            "rating" => query.OrderByDescending(a => a.Rating),
            "new" => query.OrderByDescending(a => a.CreatedAtUtc),
            _ => query.OrderByDescending(a => a.AvailableNow).ThenByDescending(a => a.Rating)
        };

        const int pageSize = 12;
        var page = vm.Page < 1 ? 1 : vm.Page;
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var govs = await _db.Governorates.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        ViewBag.Governorates = govs;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Query = vm;

        return View(items);
    }
}

public class SearchVm
{
    public string? Q { get; set; }
    public int? GovernorateId { get; set; }
    public int? NeighborhoodId { get; set; }
    public bool? AvailableNow { get; set; }
    public double? MinRating { get; set; }
    public string? Sort { get; set; } // rating | new | default
    public int Page { get; set; } = 1;
}
