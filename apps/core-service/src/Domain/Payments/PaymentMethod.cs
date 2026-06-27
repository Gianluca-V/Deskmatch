using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Payments;

public class PaymentMethod : Entity<Guid>
{
    public PaymentMethod(Guid id)
    {
        Id = id;
    }

    private PaymentMethod() { } // EF Core

    public string UserId { get; set; } = null!;
    public string LastFourDigits { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = null!;
    public bool IsDefault { get; set; } = false;
}
