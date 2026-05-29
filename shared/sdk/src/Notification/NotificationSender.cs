using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeskMatch.SDK.Notification;

public class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@deskmatch.com";
    public string FromName { get; set; } = "DeskMatch";
}

public class NotificationSender : INotificationSender
{
    private readonly ILogger<NotificationSender> _logger;
    private readonly SmtpOptions _options;

    public NotificationSender(ILogger<NotificationSender> logger, IOptions<SmtpOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        _logger.LogInformation(
            "[NOTIFICATION] Email sent to {To} | Subject: {Subject} | HTML: {IsHtml} | Body: {Body}",
            to, subject, isHtml, body);

        return Task.CompletedTask;
    }

    public Task SendTemplateEmailAsync(string to, string templateKey, Dictionary<string, string> data)
    {
        _logger.LogInformation(
            "[NOTIFICATION] Template email sent to {To} | Template: {TemplateKey} | Data: {Data}",
            to, templateKey, string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}")));

        return Task.CompletedTask;
    }
}