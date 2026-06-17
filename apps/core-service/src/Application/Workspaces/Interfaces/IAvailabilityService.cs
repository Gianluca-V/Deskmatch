using DeskMatch.CoreService.Application.Workspaces.Dtos;

namespace DeskMatch.CoreService.Application.Workspaces.Interfaces;

public interface IAvailabilityService
{
    Task<WorkspaceScheduleResponse> CreateScheduleAsync(Guid workspaceId, Guid userId, UpdateWorkspaceScheduleRequest request, CancellationToken cancellationToken);
    Task<WorkspaceScheduleResponse> UpdateScheduleAsync(Guid workspaceId, Guid scheduleId, Guid userId, UpdateWorkspaceScheduleRequest request, CancellationToken cancellationToken);
    Task DeleteScheduleAsync(Guid workspaceId, Guid scheduleId, Guid userId, CancellationToken cancellationToken);
}