// Register gateway-specific services here.
// Called from Program.cs via: builder.Services.AddGatewayServices(builder.Configuration);

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.ApiGateway.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add reverse proxy (already in Program.cs, keep there)
        // TODO: Add any custom gateway middleware here

        return services;
    }
}