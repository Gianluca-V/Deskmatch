namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record WorkspaceScheduleResponse(
    Guid Id,
    Guid WorkspaceId,
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsAvailable
);