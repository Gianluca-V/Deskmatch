using Microsoft.OpenApi.Models;

namespace DeskMatch.ApiGateway.Api.Extensions;

public static class SwaggerExtensions
{
    private static readonly Dictionary<string, string> DownstreamServices = new()
    {
        ["auth"] = "/api/auth/swagger/v1/swagger.json",
        ["core"] = "/api/core/swagger/v1/swagger.json",
        ["search"] = "/api/search/swagger/v1/swagger.json",
        ["notifications"] = "/api/notifications/swagger/v1/swagger.json"
    };

    public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
            options.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
            {
                swaggerDoc.Servers = new List<OpenApiServer>
                {
                    new() { Url = $"{httpRequest.Scheme}://{httpRequest.Host.Value}" }
                };
            });
        });

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = string.Empty;

            foreach (var (name, url) in DownstreamServices)
            {
                options.SwaggerEndpoint(url, $"{name} Service");
            }
        });

        return app;
    }
}