namespace DeskMatch.CoreService.Application.Dashboard.Dtos;

public sealed record RecentReservationDto(
    Guid Id,
    string WorkspaceName,
    DateTime CreatedAt,
    string Status
);
