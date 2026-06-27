using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Payments.Commands;

public sealed record PayFinalCommand(
    Guid ReservationId,
    string UserId,
    string PaymentMethodId) : ICommand<Guid>;
