namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record UpdateWeeklyScheduleRequest(
    List<UpdateWorkspaceScheduleRequest> Days
);