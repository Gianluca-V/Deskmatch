using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record DeleteWorkspaceScheduleCommand(
    Guid WorkspaceId,
    Guid ScheduleId,
    Guid UserId) : ICommand;