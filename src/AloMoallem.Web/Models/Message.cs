namespace AloMoallem.Web.Models;

public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    public string SenderUserId { get; set; } = "";
    public AppUser SenderUser { get; set; } = default!;

    public string Text { get; set; } = "";
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}
