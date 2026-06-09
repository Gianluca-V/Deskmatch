using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.OpenSearch.Documents;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class DeleteWorkspaceCommandHandler : ICommandHandler<DeleteWorkspaceCommand>
{
    private readonly IWorkspaceRepository _repository;
    private readonly IOpenSearchRepository<WorkspaceDocument> _searchRepo;

    public DeleteWorkspaceCommandHandler(
        IWorkspaceRepository repository,
        IOpenSearchRepository<WorkspaceDocument> searchRepo)
    {
        _repository = repository;
        _searchRepo = searchRepo;
    }

    public async Task HandleAsync(
        DeleteWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _repository.GetByIdAsync(command.Id, cancellationToken);

        workspace!.MarkAsDeleted();

        _repository.Update(workspace);
        await _repository.SaveChangesAsync(cancellationToken);

        await _searchRepo.DeleteAsync(workspace.Id.ToString(), index: "offices");
    }
}
