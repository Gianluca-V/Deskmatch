using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DeskMatch.BuildingBlocks.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddBuildingBlockAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration.GetValue<string>("Auth:Authority");
        var audience = configuration.GetValue<string>("Auth:Audience");

        if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(audience))
        {
            return services;
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.Authority = authority;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authority,
                    ValidAudience = audience
                };
            });

        return services;
    }

    public static IServiceCollection AddBuildingBlockAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("EmployerOnly", policy => policy.RequireRole("Employer"))
            .AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"))
            .AddPolicy("EmployerOrEmployee", policy => policy.RequireRole("Employer", "Employee"))
            .AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());

        return services;
    }
}