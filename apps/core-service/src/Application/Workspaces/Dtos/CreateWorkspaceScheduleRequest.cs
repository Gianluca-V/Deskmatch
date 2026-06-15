namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record CreateWorkspaceScheduleRequest(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsAvailable
);