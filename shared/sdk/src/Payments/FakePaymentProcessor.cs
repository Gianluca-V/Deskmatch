namespace DeskMatch.SDK.Payments;

public sealed class FakePaymentProcessor : IPaymentProcessor
{
    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string? customerId = null,
        string? paymentMethodId = null,
        CancellationToken cancellationToken = default)
    {
        var paymentIntentId = $"pi_fake_{Guid.NewGuid()}";
        var clientSecret = $"pi_fake_{Guid.NewGuid()}_secret";

        var result = new PaymentIntentResult(
            PaymentIntentId: paymentIntentId,
            ClientSecret: clientSecret,
            Status: "succeeded",
            Amount: amount,
            Currency: currency,
            Success: true,
            ErrorMessage: null);

        return Task.FromResult(result);
    }

    public Task<PaymentIntentResult> CapturePaymentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var result = new PaymentIntentResult(
            PaymentIntentId: paymentIntentId,
            ClientSecret: null,
            Status: "succeeded",
            Amount: 0,
            Currency: "ARS",
            Success: true,
            ErrorMessage: null);

        return Task.FromResult(result);
    }

    public Task<PaymentIntentResult> ProcessRefundAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default)
    {
        var result = new PaymentIntentResult(
            PaymentIntentId: paymentIntentId,
            ClientSecret: null,
            Status: "succeeded",
            Amount: amount ?? 0,
            Currency: "ARS",
            Success: true,
            ErrorMessage: null);

        return Task.FromResult(result);
    }

    public Task<CustomerDto> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default)
    {
        var customerId = $"cus_fake_{Guid.NewGuid()}";
        var result = new CustomerDto(
            CustomerId: customerId,
            Email: email,
            Name: name);

        return Task.FromResult(result);
    }
}
