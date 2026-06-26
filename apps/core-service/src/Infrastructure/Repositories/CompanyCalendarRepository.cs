using DeskMatch.CoreService.Application.CompanyCalendar.Dtos;
using DeskMatch.CoreService.Application.CompanyCalendar.Interfaces;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public sealed class CompanyCalendarRepository : ICompanyCalendarRepository
{
    private readonly CoreDbContext _context;

    public CompanyCalendarRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompanyCalendarEntryDto>> GetReservationsInRangeAsync(
        Guid companyId, DateTimeOffset monthStart, DateTimeOffset monthEnd, CancellationToken ct)
    {
        var reservations = await (
            from r in _context.Reservations.AsNoTracking()
            join w in _context.Workspaces on r.WorkspaceId equals w.Id
            where w.CompanyId == companyId
            where (r.StartTime >= monthStart && r.StartTime <= monthEnd)
               || (r.EndTime >= monthStart && r.EndTime <= monthEnd)
            where r.Status == ReservationStatus.Confirmed
               || r.Status == ReservationStatus.Completed
            orderby r.StartTime ascending
            select new CompanyCalendarEntryDto(
                r.Id,
                w.Id,
                w.Name,
                r.StartTime,
                r.EndTime,
                r.TotalPrice,
                r.Status.ToString()
            )
        ).ToListAsync(ct);

        return reservations;
    }
}
