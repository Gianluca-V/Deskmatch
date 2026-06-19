namespace DeskMatch.CoreService.Application.Reservations.Dtos;

public sealed record CompanyReservationSummaryDto(
    int Total,
    int Confirmed,
    int Cancelled,
    int Completed,
    int Upcoming,
    int ThisMonth,
    decimal RevenueThisMonth);
