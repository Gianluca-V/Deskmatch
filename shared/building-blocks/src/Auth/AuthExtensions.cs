using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DeskMatch.BuildingBlocks.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddBuildingBlockAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = FirstConfiguredValue(configuration, "JWT_SECRET", "Jwt:Secret");
        var issuer = FirstConfiguredValue(configuration, "JWT_ISSUER", "Jwt:Issuer");
        var audience = FirstConfiguredValue(configuration, "JWT_AUDIENCE", "Jwt:Audience", "Auth:Audience");

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var authBuilder = services.AddAuthentication();

        if (!string.IsNullOrWhiteSpace(secret) &&
            !string.IsNullOrWhiteSpace(issuer) &&
            !string.IsNullOrWhiteSpace(audience))
        {
            authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        }

        return services;
    }

    private static string? FirstConfiguredValue(IConfiguration configuration, params string[] keys)
    {
        return keys
            .Select(key => configuration[key])
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    public static IServiceCollection AddBuildingBlockAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"))
            .AddPolicy("UserOnly", policy => policy.RequireRole("User"))
            .AddPolicy("ManagerOrUser", policy => policy.RequireRole("Manager", "User"))
            .AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
