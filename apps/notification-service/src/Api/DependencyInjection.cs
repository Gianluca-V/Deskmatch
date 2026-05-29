// Register your application services here.
// Called from Program.cs via: builder.Services.AddApplicationServices(builder.Configuration);

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.NotificationService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add DbContext (Npgsql)
        // services.AddDbContext<NotificationDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // TODO: Configure SMTP options
        // services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        // TODO: Register repositories
        // services.AddScoped<INotificationRepository, NotificationRepository>();
        // services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

        // TODO: Register services
        // services.AddScoped<SmtpEmailSender>();

        // TODO: Register command/query handlers
        // services.AddScoped<ICommandHandler<SendEmailCommand, Guid>, SendEmailCommandHandler>();
        // services.AddScoped<ICommandHandler<SendTemplateEmailCommand, Guid>, SendTemplateEmailCommandHandler>();
        // services.AddScoped<IQueryHandler<GetNotificationByIdQuery, NotificationDto?>, GetNotificationByIdQueryHandler>();

        return services;
    }
}