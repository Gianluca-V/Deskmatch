// Register your application services here.
// Called from Program.cs via: builder.Services.AddApplicationServices(builder.Configuration);

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.AuthService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add DbContext (Npgsql)
        // services.AddDbContext<AuthDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // TODO: Register JWT options
        // services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        // services.AddAuthentication()...AddJwtBearer()...

        // TODO: Register repositories
        // services.AddScoped<IUserRepository, UserRepository>();

        // TODO: Register services
        // services.AddSingleton<IPasswordHasher, PasswordHasher>();
        // services.AddScoped<IJwtService, JwtService>();

        // TODO: Register command handlers
        // services.AddScoped<ICommandHandler<RegisterCommand, AuthResponse>, RegisterCommandHandler>();
        // services.AddScoped<ICommandHandler<LoginCommand, AuthResponse>, LoginCommandHandler>();

        // TODO: Register query handlers
        // services.AddScoped<IQueryHandler<GetUserByIdQuery, UserDto>, GetUserByIdQueryHandler>();

        return services;
    }
}