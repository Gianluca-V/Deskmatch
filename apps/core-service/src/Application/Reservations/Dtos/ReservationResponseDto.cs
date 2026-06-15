namespace DeskMatch.CoreService.Application.Reservations.Dtos;

public sealed record ReservationResponseDto(
    Guid Id,
    Guid WorkspaceId,
    Guid GuestId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TotalPrice,
    int Status,
    DateTime CreatedAt,
    string? WorkspaceName = null);
