namespace DeskMatch.CoreService.Application.Dashboard.Dtos;

public sealed record DailyReservationDto(
    string Date,
    int Count
);
