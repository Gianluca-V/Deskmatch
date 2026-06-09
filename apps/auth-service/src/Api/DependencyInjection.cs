using DeskMatch.AuthService.Application.Auth;
using DeskMatch.AuthService.Application.Auth.Validators;
using DeskMatch.AuthService.Application.Users;
using DeskMatch.AuthService.Infrastructure.Auth;
using DeskMatch.AuthService.Infrastructure.Identity;
using DeskMatch.AuthService.Infrastructure.Persistence;
using DeskMatch.SDK.Redis;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.AuthService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        // AddIdentity registra cookie auth como esquema default; lo sobreescribimos para que
        // los endpoints de API usen JWT y devuelvan 401 en vez de redirigir a /Account/Login.
        services.Configure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddRedisSdk(configuration);
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }
}
