namespace DeskMatch.CoreService.Application.Dashboard.Dtos;

public sealed record DashboardResponseDto(
    decimal TotalRevenue,
    int ActiveReservationsCount,
    int TotalWorkspacesCount,
    IReadOnlyList<string> PopularWorkspaces,
    IReadOnlyList<RecentReservationDto> RecentActivity,
    IReadOnlyList<DailyReservationDto> DailyReservationsChart,
    IReadOnlyList<WorkspaceRevenueDto> RevenueByWorkspaceChart
);
