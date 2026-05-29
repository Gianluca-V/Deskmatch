using DeskMatch.BuildingBlocks.Auth;
using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.BuildingBlocks.Logging;
using DeskMatch.BuildingBlocks.Middleware;
using DeskMatch.BuildingBlocks.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeskMatch.BuildingBlocks.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBuildingBlockAuth(configuration);
        services.AddBuildingBlockAuthorization();
        services.AddBuildingBlockObservability(configuration);

        return services;
    }

    public static IHostBuilder UseBuildingBlocks(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        hostBuilder.AddBuildingBlockLogging(configuration);

        return hostBuilder;
    }

    public static IApplicationBuilder UseBuildingBlocksMiddleware(this IApplicationBuilder app)
    {
        app.UseCorrelationId();
        app.UseRequestLogging();
        app.UseBuildingBlockExceptionHandler();

        return app;
    }
}