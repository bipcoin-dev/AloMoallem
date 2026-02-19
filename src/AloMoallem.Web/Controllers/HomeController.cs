using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        q = (q ?? "").Trim();

        var professionsQuery = _db.Professions.AsNoTracking().OrderBy(p => p.Name);
        var professions = await professionsQuery.ToListAsync();

        // شريط بحث: إذا كتب المستخدم اسم مهنة/خدمة، نوديه لنتائج حرفيين
        if (!string.IsNullOrWhiteSpace(q))
        {
            return RedirectToAction("Results", "Search", new { q });
        }

        return View(professions);
    }
}
