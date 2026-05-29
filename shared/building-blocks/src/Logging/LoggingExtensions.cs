using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.CorrelationId;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;

namespace DeskMatch.BuildingBlocks.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder AddBuildingBlockLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        var lokiEndpoint = configuration.GetValue<string>("Serilog:Loki:Uri");

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", configuration.GetValue<string>("Application:Name") ?? "DeskMatch")
            .WriteTo.Console(new ElasticsearchJsonFormatter())
            .WriteTo.GrafanaLoki(
                lokiEndpoint ?? "http://localhost:3100",
                labels: new[]
                {
                    new LokiLabel { Key = "app", Value = "DeskMatch" },
                    new LokiLabel { Key = "environment", Value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development" }
                },
                propertiesAsLabels: new[] { "level" }
            )
            .CreateLogger();

        return hostBuilder.UseSerilog();
    }
}