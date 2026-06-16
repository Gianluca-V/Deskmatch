using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly CoreDbContext _context;

    public ReservationRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasOverlapAsync(
        Guid workspaceId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.AnyAsync(
            r => r.WorkspaceId == workspaceId
              && r.Status == ReservationStatus.Confirmed
              && r.StartTime < end
              && r.EndTime > start,
            cancellationToken);
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetByGuestIdAsync(
        Guid guestId,
        CancellationToken cancellationToken = default)
        => await _context.Reservations
            .Where(r => r.GuestId == guestId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
        => await _context.Reservations
            .Where(r => r.WorkspaceId == workspaceId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
        => await (
            from reservation in _context.Reservations
            join workspace in _context.Workspaces on reservation.WorkspaceId equals workspace.Id
            where workspace.CompanyId == companyId
            orderby reservation.StartTime descending
            select reservation)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        => await _context.Reservations.AddAsync(reservation, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
