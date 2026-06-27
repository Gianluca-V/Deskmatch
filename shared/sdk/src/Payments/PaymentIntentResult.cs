namespace DeskMatch.SDK.Payments;

public record PaymentIntentResult(
    string PaymentIntentId,
    string? ClientSecret,
    string Status,
    decimal Amount,
    string Currency,
    bool Success,
    string? ErrorMessage = null);
