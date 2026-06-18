using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record CreateWorkspaceBlockCommand(
    Guid WorkspaceId,
    Guid UserId,
    DateTimeOffset BlockStart,
    DateTimeOffset BlockEnd,
    string? Reason) : ICommand<Guid>;