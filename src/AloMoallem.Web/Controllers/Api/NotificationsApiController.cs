using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers.Api;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public NotificationsApiController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Unauthorized();

        var items = await _db.AppNotifications.AsNoTracking()
            .Where(n => n.UserId == me.Id)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(10)
            .Select(n => new { n.Id, n.Title, n.Body, n.Url, n.IsRead, n.CreatedAtUtc })
            .ToListAsync();

        var unread = await _db.AppNotifications.CountAsync(n => n.UserId == me.Id && !n.IsRead);

        return Ok(new { unread, items });
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Unauthorized();

        var n = await _db.AppNotifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == me.Id);
        if (n is null) return NotFound();

        n.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok();
    }
}
