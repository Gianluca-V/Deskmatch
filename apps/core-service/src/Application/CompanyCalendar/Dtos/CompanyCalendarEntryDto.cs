namespace DeskMatch.CoreService.Application.CompanyCalendar.Dtos;

public sealed record CompanyCalendarEntryDto(
    Guid Id,
    Guid WorkspaceId,
    string WorkspaceName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TotalPrice,
    string Status
);
