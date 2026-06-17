using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record UpdateWorkspaceScheduleCommand(
    Guid WorkspaceId,
    Guid ScheduleId,
    Guid UserId,
    DayOfWeek DayOfWeek,
    string OpenTime,
    string CloseTime,
    bool IsAvailable) : ICommand;