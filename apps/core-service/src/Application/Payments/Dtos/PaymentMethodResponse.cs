namespace DeskMatch.CoreService.Application.Payments.Dtos;

public record PaymentMethodResponse(
    Guid Id,
    string LastFourDigits,
    int ExpiryMonth,
    int ExpiryYear,
    string CardHolderName,
    bool IsDefault,
    DateTime CreatedAt);
