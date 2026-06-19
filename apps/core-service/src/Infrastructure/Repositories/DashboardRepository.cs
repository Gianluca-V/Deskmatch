using DeskMatch.CoreService.Application.Dashboard.Dtos;
using DeskMatch.CoreService.Application.Dashboard.Interfaces;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly CoreDbContext _context;

    public DashboardRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsHostAsync(Guid hostId, CancellationToken ct)
        => await _context.Companies
            .AsNoTracking()
            .AnyAsync(c => c.OwnerId == hostId && c.IsActive, ct);

    public async Task<DashboardKpisDto> GetKpisAsync(Guid hostId, CancellationToken ct)
    {
        // Query base: reservas del host a través de la cadena Company → Workspace → Reservation
        var reservationsQuery = from reservation in _context.Reservations.AsNoTracking()
                                join workspace in _context.Workspaces on reservation.WorkspaceId equals workspace.Id
                                join company in _context.Companies on workspace.CompanyId equals company.Id
                                where company.OwnerId == hostId
                                select reservation;

        var totalRevenue = await reservationsQuery
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .SumAsync(r => r.TotalPrice, ct);

        var activeReservationsCount = await reservationsQuery
            .Where(r => r.Status == ReservationStatus.Confirmed && r.EndTime > DateTimeOffset.UtcNow)
            .CountAsync(ct);

        var totalWorkspacesCount = await _context.Workspaces
            .AsNoTracking()
            .Where(w => w.IsActive)
            .Join(_context.Companies.Where(c => c.OwnerId == hostId && c.IsActive),
                  w => w.CompanyId,
                  c => c.Id,
                  (w, _) => w)
            .CountAsync(ct);

        return new DashboardKpisDto(totalRevenue, activeReservationsCount, totalWorkspacesCount);
    }

    public async Task<IReadOnlyList<string>> GetPopularWorkspacesAsync(Guid hostId, int limit, CancellationToken ct)
    {
        var popular = await (
            from reservation in _context.Reservations.AsNoTracking()
            join workspace in _context.Workspaces on reservation.WorkspaceId equals workspace.Id
            join company in _context.Companies on workspace.CompanyId equals company.Id
            where company.OwnerId == hostId
            group reservation by workspace.Name into g
            orderby g.Count() descending
            select g.Key
        ).Take(limit).ToListAsync(ct);

        return popular;
    }

    public async Task<IReadOnlyList<RecentReservationDto>> GetRecentReservationsAsync(Guid hostId, int limit, CancellationToken ct)
    {
        var recent = await (
            from reservation in _context.Reservations.AsNoTracking()
            join workspace in _context.Workspaces on reservation.WorkspaceId equals workspace.Id
            join company in _context.Companies on workspace.CompanyId equals company.Id
            where company.OwnerId == hostId
            orderby reservation.CreatedAt descending
            select new RecentReservationDto(
                reservation.Id,
                workspace.Name,
                reservation.CreatedAt,
                reservation.Status.ToString()
            )
        ).Take(limit).ToListAsync(ct);

        return recent;
    }

    public async Task<IReadOnlyList<DailyReservationDto>> GetDailyReservationsTrendAsync(Guid hostId, DateTime startDate, CancellationToken ct)
    {
        // Primero agrupamos y contamos en SQL; formateamos la fecha en memoria.
        var query = from reservation in _context.Reservations.AsNoTracking()
                    join workspace in _context.Workspaces on reservation.WorkspaceId equals workspace.Id
                    join company in _context.Companies on workspace.CompanyId equals company.Id
                    where company.OwnerId == hostId
                    where reservation.CreatedAt >= startDate
                    select reservation;

        var rawData = await query
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rawData
            .Select(x => new DailyReservationDto(x.Date.ToString("yyyy-MM-dd"), x.Count))
            .ToList();
    }

    public async Task<IReadOnlyList<WorkspaceRevenueDto>> GetRevenueGroupedByWorkspaceAsync(Guid hostId, CancellationToken ct)
    {
        var revenueData = await (
            from reservation in _context.Reservations.AsNoTracking()
            join workspace in _context.Workspaces on reservation.WorkspaceId equals workspace.Id
            join company in _context.Companies on workspace.CompanyId equals company.Id
            where company.OwnerId == hostId
            where reservation.Status != ReservationStatus.Cancelled
            select new { workspace.Name, reservation.TotalPrice }
        ).GroupBy(x => x.Name)
        .Select(g => new WorkspaceRevenueDto(g.Key, g.Sum(x => x.TotalPrice)))
        .ToListAsync(ct);

        return revenueData;
    }
}
