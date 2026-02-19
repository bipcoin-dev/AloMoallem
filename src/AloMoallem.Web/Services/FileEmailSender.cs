using System.Text;
using System.Linq;

namespace AloMoallem.Web.Services;

public class FileEmailSender : IEmailSender
{
    private readonly IWebHostEnvironment _env;

    public FileEmailSender(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        // حفظ الإيميل كملف (للتطوير المحلي)
        var folder = Path.Combine(_env.ContentRootPath, "App_Data", "emails");
        Directory.CreateDirectory(folder);

        var safeTo = string.Concat((toEmail ?? "unknown").Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
        var file = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeTo}.html";

        var content = new StringBuilder();
        content.AppendLine("<meta charset=\"utf-8\" />");
        content.AppendLine($"<h2>{System.Net.WebUtility.HtmlEncode(subject)}</h2>");
        content.AppendLine($"<div><b>To:</b> {System.Net.WebUtility.HtmlEncode(toEmail)}</div>");
        content.AppendLine("<hr/>");
        content.AppendLine(htmlBody);

        await File.WriteAllTextAsync(Path.Combine(folder, file), content.ToString(), Encoding.UTF8);
    }
}
