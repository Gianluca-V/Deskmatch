namespace DeskMatch.CoreService.Application.Payments.Dtos;

public record PaymentResponse(
    Guid PaymentId,
    Guid ReservationId,
    decimal Amount,
    int PaymentType,
    int Status,
    DateTimeOffset? PaidAt,
    DateTime CreatedAt);
