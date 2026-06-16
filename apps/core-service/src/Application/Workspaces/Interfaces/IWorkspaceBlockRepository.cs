using DeskMatch.CoreService.Domain.Workspaces;

namespace DeskMatch.CoreService.Application.Workspaces.Interfaces;

public interface IWorkspaceBlockRepository
{
    Task<IEnumerable<WorkspaceBlock>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken);
    Task<WorkspaceBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> AddAsync(WorkspaceBlock block, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}