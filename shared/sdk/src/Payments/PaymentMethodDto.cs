namespace DeskMatch.SDK.Payments;

public record PaymentMethodDto(
    string Id,
    string LastFourDigits,
    int ExpiryMonth,
    int ExpiryYear,
    string CardHolderName,
    bool IsDefault);
