using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record UpdateWeeklyScheduleCommand(
    Guid WorkspaceId,
    Guid UserId,
    List<DayScheduleItem> Days) : ICommand;

public sealed record DayScheduleItem(
    DayOfWeek DayOfWeek,
    string OpenTime,
    string CloseTime,
    bool IsAvailable);