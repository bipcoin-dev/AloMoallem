using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ChatHub(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private static string GroupName(int conversationId) => $"convo-{conversationId}";

    public async Task JoinConversation(int conversationId)
    {
        var me = await _userManager.GetUserAsync(Context.User!);
        if (me is null) throw new HubException("Unauthorized");

        var convo = await _db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId);
        if (convo is null) throw new HubException("Conversation not found");

        if (convo.CustomerUserId != me.Id && convo.ArtisanUserId != me.Id)
            throw new HubException("Forbidden");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId));
    }

    public async Task SendMessage(int conversationId, string text)
    {
        var me = await _userManager.GetUserAsync(Context.User!);
        if (me is null) throw new HubException("Unauthorized");

        text = (text ?? "").Trim();
        if (text.Length == 0) return;
        if (text.Length > 2000) text = text[..2000];

        var convo = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
        if (convo is null) throw new HubException("Conversation not found");
        if (convo.CustomerUserId != me.Id && convo.ArtisanUserId != me.Id)
            throw new HubException("Forbidden");

        var msg = new Message
        {
            ConversationId = conversationId,
            SenderUserId = me.Id,
            Text = text,
            SentAtUtc = DateTime.UtcNow
        };

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        await Clients.Group(GroupName(conversationId)).SendAsync("message", new
        {
            conversationId,
            senderEmail = me.Email ?? me.UserName ?? "",
            senderUserId = me.Id,
            text = msg.Text,
            sentAtUtc = msg.SentAtUtc
        });
    }
}
