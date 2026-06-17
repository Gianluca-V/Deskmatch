using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Reservations.Commands;

public sealed record CancelReservationCommand(
    Guid ReservationId,
    Guid GuestId) : ICommand;
