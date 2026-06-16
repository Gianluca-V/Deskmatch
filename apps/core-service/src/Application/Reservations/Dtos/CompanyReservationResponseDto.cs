namespace DeskMatch.CoreService.Application.Reservations.Dtos;

public sealed record CompanyReservationResponseDto(
    Guid Id,
    Guid WorkspaceId,
    string? WorkspaceName,
    Guid GuestId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TotalPrice,
    int Status,
    DateTime CreatedAt);
