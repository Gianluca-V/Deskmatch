using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Payments.Commands;

public sealed record PayDepositCommand(
    Guid ReservationId,
    string UserId,
    string PaymentMethodId) : ICommand<Guid>;
