namespace DeskMatch.CoreService.Application.Reservations.Dtos;

public sealed record CreateReservationRequestDto(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
