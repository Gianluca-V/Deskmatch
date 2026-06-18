using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record DeleteWorkspaceBlockCommand(
    Guid WorkspaceId,
    Guid BlockId,
    Guid UserId) : ICommand;