using DeskMatch.CoreService.Domain.Workspaces;

namespace DeskMatch.CoreService.Application.Workspaces.Interfaces;

public interface IWorkspaceScheduleRepository
{
    Task<IEnumerable<WorkspaceSchedule>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken);
    Task<WorkspaceSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> AddAsync(WorkspaceSchedule schedule, CancellationToken cancellationToken);
    Task UpdateAsync(WorkspaceSchedule schedule, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}