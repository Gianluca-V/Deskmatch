using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Reservations.Commands;
using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Reservations.Handlers;

public sealed class CancelReservationCommandHandler : ICommandHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _repository;

    public CancelReservationCommandHandler(IReservationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        CancelReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _repository.GetByIdAsync(command.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Reservation", command.ReservationId);

        if (reservation.GuestId != command.GuestId)
            throw new ForbiddenException("Solo podés cancelar tus propias reservas.");

        if (reservation.Status != ReservationStatus.Confirmed)
            throw new BadRequestException("Solo se pueden cancelar reservas confirmadas.");

        if (reservation.StartTime <= DateTimeOffset.UtcNow)
            throw new BadRequestException("No se pueden cancelar reservas cuyo inicio ya pasó.");

        reservation.Status = ReservationStatus.Cancelled;
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
