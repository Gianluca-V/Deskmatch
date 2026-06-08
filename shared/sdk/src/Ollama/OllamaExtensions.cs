using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SDK.Ollama;

public static class OllamaExtensions
{
    public static IServiceCollection AddOllamaClient(this IServiceCollection services)
    {
        services.AddSingleton<IOllamaClient, OllamaClient>();
        return services;
    }
}
