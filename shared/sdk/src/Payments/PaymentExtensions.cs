using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.SDK.Payments;

public static class PaymentExtensions
{
    public static IServiceCollection AddPaymentSdk(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Payment:Provider") ?? "fake";

        if (provider.Equals("fake", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IPaymentProcessor, FakePaymentProcessor>();
        }
        else
        {
            throw new NotSupportedException($"Payment provider '{provider}' is not supported.");
        }

        return services;
    }
}
