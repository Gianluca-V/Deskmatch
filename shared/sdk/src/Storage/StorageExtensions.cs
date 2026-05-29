using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SDK.Storage;

public static class StorageExtensions
{
    public static IServiceCollection AddStorageSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IStorageService, LocalStorageService>();

        return services;
    }
}