using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SearchService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenSearchSdk(configuration);
        services.AddOllamaClient();

        return services;
    }
}
