using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SDK.Notification;

public static class NotificationExtensions
{
    public static IServiceCollection AddNotificationSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddSingleton<INotificationSender, NotificationSender>();

        return services;
    }
}