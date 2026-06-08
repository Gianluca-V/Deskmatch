using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using DeskMatch.SearchService.Application.Search;
using OpenSearch.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SearchService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenSearchSdk(configuration);
        services.AddOllamaClient();

        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<IOpenSearchClient>();
            return new OpenSearchRepository<WorkspaceDocument>(client);
        });

        services.AddTransient<IQueryHandler<SearchOfficesQuery, SearchOfficesResponse>, SearchOfficesQueryHandler>();

        return services;
    }
}
