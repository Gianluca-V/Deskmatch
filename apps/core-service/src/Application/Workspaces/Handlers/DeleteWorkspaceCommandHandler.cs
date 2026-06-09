using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class DeleteWorkspaceCommandHandler : ICommandHandler<DeleteWorkspaceCommand>
{
    private readonly IWorkspaceRepository _repository;

    public DeleteWorkspaceCommandHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeleteWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _repository.GetByIdAsync(command.Id, cancellationToken);

        workspace!.MarkAsDeleted();

        _repository.Update(workspace);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
