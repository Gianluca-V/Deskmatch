using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.Abstractions;

namespace DeskMatch.CoreService.Application.Workspaces.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace, Guid>
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}