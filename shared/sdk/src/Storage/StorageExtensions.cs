using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SDK.Storage;

public static class StorageExtensions
{
    public static IServiceCollection AddStorageSdk(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Storage:Provider") ?? "local";

        services.Configure<S3StorageOptions>(configuration.GetSection("Storage"));

        if (provider.Equals("minio", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("s3", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IStorageService, S3StorageService>();
        }
        else
        {
            services.AddSingleton<IStorageService, LocalStorageService>();
        }

        return services;
    }
}