using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AloMoallem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminProfessionsController : Controller
{
    private readonly AppDbContext _db;
    public AdminProfessionsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateProfessionVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProfessionVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var exists = await _db.Professions.AnyAsync(p => p.Name == vm.Name.Trim());
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Name), "هذه المهنة موجودة مسبقاً");
            return View(vm);
        }

        _db.Professions.Add(new Profession
        {
            Name = vm.Name.Trim(),
            Icon = string.IsNullOrWhiteSpace(vm.Icon) ? "🛠️" : vm.Icon.Trim(),
            Description = vm.Description?.Trim() ?? ""
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

public class CreateProfessionVm
{
    [Required]
    public string Name { get; set; } = "";

    public string Icon { get; set; } = "🛠️";
    public string? Description { get; set; }
}
