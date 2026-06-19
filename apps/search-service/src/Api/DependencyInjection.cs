// Register your application services here.
// Called from Program.cs via: builder.Services.AddApplicationServices(builder.Configuration);

using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SearchService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add OpenSearch SDK
        services.AddOpenSearchSdk(configuration);

        services.AddOllamaClient();

        services.AddRedisSdk(configuration);
        services.AddSingleton<ICacheService, CacheService>();

        // TODO: Register document repositories
        // services.AddSingleton(typeof(IOpenSearchRepository<>), typeof(OpenSearchRepository<>));
        // services.AddSingleton(provider =>
        // {
        //     var client = provider.GetRequiredService<IOpenSearchClient>();
        //     return new OpenSearchRepository<OfficeDocument>(client);
        // });

        // TODO: Register indexer service
        // services.AddScoped<OpenSearchInitializer>();

        // TODO: Register command/query handlers
        // services.AddTransient<ICommandHandler<IndexOfficeCommand>, IndexOfficeCommandHandler>();
        // services.AddTransient<ICommandHandler<RemoveOfficeIndexCommand>, RemoveOfficeIndexCommandHandler>();
        // services.AddTransient<IQueryHandler<SearchOfficesQuery, SearchOfficesResponse>, SearchOfficesQueryHandler>();

        return services;
    }
}
