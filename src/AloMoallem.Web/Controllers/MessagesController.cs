using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Controllers;

[Authorize]
public class MessagesController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var q = _db.Conversations.AsNoTracking().OrderByDescending(c => c.UpdatedAtUtc).AsQueryable();

        if (User.IsInRole("Artisan")) q = q.Where(c => c.ArtisanUserId == me.Id);
        else q = q.Where(c => c.CustomerUserId == me.Id);

        var list = await q.Take(50).ToListAsync();
        return View(list);
    }

    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public MessagesController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("/chat/{artisanUserId}")]
    public async Task<IActionResult> Chat(string artisanUserId)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        var convo = await _db.Conversations
            .Include(c => c.Messages.OrderBy(m => m.SentAtUtc))
            .ThenInclude(m => m.SenderUser)
            .FirstOrDefaultAsync(c => c.CustomerUserId == me.Id && c.ArtisanUserId == artisanUserId);

        if (convo is null)
        {
            convo = new Conversation { CustomerUserId = me.Id, ArtisanUserId = artisanUserId, UpdatedAtUtc = DateTime.UtcNow };
            _db.Conversations.Add(convo);
            await _db.SaveChangesAsync();
        }

        return View(convo);
    }

    [HttpPost("/chat/{conversationId:int}/send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int conversationId, string text)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me is null) return Challenge();

        text = (text ?? "").Trim();
        if (text.Length == 0) return RedirectToAction("Index", "Home");

        var convo = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
        if (convo is null) return NotFound();

        if (convo.CustomerUserId != me.Id && convo.ArtisanUserId != me.Id) return Forbid();

        _db.Messages.Add(new Message { ConversationId = conversationId, SenderUserId = me.Id, Text = text });

        // keep conversation sorted by recent activity
        convo.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var targetArtisan = convo.CustomerUserId == me.Id ? convo.ArtisanUserId : convo.CustomerUserId;
        return Redirect($"/chat/{targetArtisan}");
    }
}
