using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Payments;

public class Payment : AggregateRoot<Guid>
{
    public Payment(Guid id)
    {
        Id = id;
    }

    private Payment() { } // EF Core

    public Guid ReservationId { get; set; }
    public string UserId { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset? PaidAt { get; set; }
}
