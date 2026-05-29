using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DeskMatch.SDK.Redis;

public class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int DefaultDatabase { get; set; } = 0;
}

public static class RedisExtensions
{
    public static IServiceCollection AddRedisSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        var options = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();

        var connectionMultiplexer = ConnectionMultiplexer.Connect(options.ConnectionString);

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        return services;
    }
}