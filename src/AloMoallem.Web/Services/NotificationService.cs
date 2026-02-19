using AloMoallem.Web.Data;
using AloMoallem.Web.Hubs;
using AloMoallem.Web.Models;
using Microsoft.AspNetCore.SignalR;

namespace AloMoallem.Web.Services;

public class NotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationsHub> _hub;

    public NotificationService(AppDbContext db, IHubContext<NotificationsHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task NotifyAsync(string userId, string title, string body, string? url = null)
    {
        _db.AppNotifications.Add(new AppNotification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Url = url,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await _hub.Clients.Group(NotificationsHub.UserGroup(userId))
            .SendAsync("notify", new { title, body, url });
    }
}
