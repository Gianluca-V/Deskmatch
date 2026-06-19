using DeskMatch.CoreService.Application.Dashboard.Dtos;

namespace DeskMatch.CoreService.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetHostDashboardAsync(Guid hostId, CancellationToken cancellationToken = default);
}
