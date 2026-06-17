using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record CreateWorkspaceScheduleCommand(
    Guid WorkspaceId,
    Guid UserId,
    DayOfWeek DayOfWeek,
    string OpenTime,
    string CloseTime,
    bool IsAvailable) : ICommand<Guid>;