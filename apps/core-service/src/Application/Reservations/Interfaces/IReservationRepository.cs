using DeskMatch.CoreService.Domain.Reservations;

namespace DeskMatch.CoreService.Application.Reservations.Interfaces;

public interface IReservationRepository
{
    Task<bool> HasOverlapAsync(Guid workspaceId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reservation>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
