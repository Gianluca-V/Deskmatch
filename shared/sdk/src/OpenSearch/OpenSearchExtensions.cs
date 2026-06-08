using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using OpenSearch.Net;

namespace DeskMatch.SDK.OpenSearch;

public class OpenSearchOptions
{
    public string Uri { get; set; } = "http://localhost:9200";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DefaultIndex { get; set; } = "default";
}

public static class OpenSearchExtensions
{
    public static IServiceCollection AddOpenSearchSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenSearchOptions>(configuration.GetSection("OpenSearch"));

        var options = configuration.GetSection("OpenSearch").Get<OpenSearchOptions>() ?? new OpenSearchOptions();

        var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri(options.Uri)))
            .BasicAuthentication(options.Username, options.Password)
            .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true)
            .DefaultIndex(options.DefaultIndex)
            .DisableDirectStreaming();

        var client = new OpenSearchClient(settings);

        services.AddSingleton<IOpenSearchClient>(client);
        services.AddSingleton(typeof(IOpenSearchRepository<>), typeof(OpenSearchRepository<>));

        return services;
    }
}