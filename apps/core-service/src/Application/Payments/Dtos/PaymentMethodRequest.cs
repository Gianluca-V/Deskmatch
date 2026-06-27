namespace DeskMatch.CoreService.Application.Payments.Dtos;

public class PaymentMethodRequest
{
    public string LastFourDigits { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = null!;
    public bool IsDefault { get; set; }
}
