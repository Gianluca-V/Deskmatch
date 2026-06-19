using DeskMatch.CoreService.Application.Dashboard.Dtos;

namespace DeskMatch.CoreService.Application.Dashboard.Interfaces;

public sealed record DashboardKpisDto(
    decimal TotalRevenue,
    int ActiveReservationsCount,
    int TotalWorkspacesCount
);

public interface IDashboardRepository
{
    Task<bool> IsHostAsync(Guid hostId, CancellationToken cancellationToken = default);
    Task<DashboardKpisDto> GetKpisAsync(Guid hostId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPopularWorkspacesAsync(Guid hostId, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentReservationDto>> GetRecentReservationsAsync(Guid hostId, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyReservationDto>> GetDailyReservationsTrendAsync(Guid hostId, DateTime startDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceRevenueDto>> GetRevenueGroupedByWorkspaceAsync(Guid hostId, CancellationToken cancellationToken = default);
}
