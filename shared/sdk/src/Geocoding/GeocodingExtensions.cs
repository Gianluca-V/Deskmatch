using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SDK.Geocoding;

public static class GeocodingExtensions
{
    public static IServiceCollection AddGeocodingSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
        {
            client.BaseAddress = new Uri("https://nominatim.openstreetmap.org");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DeskMatch/1.0");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
