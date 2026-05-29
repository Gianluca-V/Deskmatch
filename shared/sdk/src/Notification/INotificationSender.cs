namespace DeskMatch.SDK.Notification;

public interface INotificationSender
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendTemplateEmailAsync(string to, string templateKey, Dictionary<string, string> data);
}