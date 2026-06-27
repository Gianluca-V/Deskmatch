using DeskMatch.CoreService.Application.CompanyCalendar.Dtos;

namespace DeskMatch.CoreService.Application.CompanyCalendar.Interfaces;

public interface ICompanyCalendarRepository
{
    Task<IReadOnlyList<CompanyCalendarEntryDto>> GetReservationsInRangeAsync(
        Guid companyId,
        DateTimeOffset monthStart,
        DateTimeOffset monthEnd,
        CancellationToken cancellationToken = default);
}
