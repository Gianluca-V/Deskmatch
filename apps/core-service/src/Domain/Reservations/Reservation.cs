using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Reservations;

public class Reservation : AggregateRoot<Guid>
{
    public Reservation(Guid id)
    {
        Id = id;
    }

    private Reservation() { } // EF Core

    public Guid WorkspaceId { get; set; }
    public Guid GuestId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public decimal DepositPercentage { get; set; } = 30;
    public decimal? DepositAmount { get; set; }
    public bool DepositPaid { get; set; } = false;
    public bool FullyPaid { get; set; } = false;
}
