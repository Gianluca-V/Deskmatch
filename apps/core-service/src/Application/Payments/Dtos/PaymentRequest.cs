namespace DeskMatch.CoreService.Application.Payments.Dtos;

public class PaymentRequest
{
    public string PaymentMethodId { get; set; } = null!;
    public bool SavePaymentMethod { get; set; }
    public string PaymentType { get; set; } = null!;
}
