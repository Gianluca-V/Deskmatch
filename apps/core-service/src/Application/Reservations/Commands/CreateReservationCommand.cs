using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Reservations.Commands;

public sealed record CreateReservationCommand(
    Guid WorkspaceId,
    Guid GuestId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime) : ICommand<Guid>;
