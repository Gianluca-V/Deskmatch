namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record UpdateWorkspaceScheduleRequest(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsAvailable
);