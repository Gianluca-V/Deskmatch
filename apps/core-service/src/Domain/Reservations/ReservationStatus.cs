namespace DeskMatch.CoreService.Domain.Reservations;

public enum ReservationStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}
