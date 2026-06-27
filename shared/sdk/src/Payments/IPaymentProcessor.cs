namespace DeskMatch.SDK.Payments;

public interface IPaymentProcessor
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string? customerId = null,
        string? paymentMethodId = null,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResult> CapturePaymentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResult> ProcessRefundAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default);
}
