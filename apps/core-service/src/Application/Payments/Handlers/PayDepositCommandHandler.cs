using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Payments.Commands;
using DeskMatch.CoreService.Application.Payments.Interfaces;
using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Domain.Payments;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Payments;

namespace DeskMatch.CoreService.Application.Payments.Handlers;

public sealed class PayDepositCommandHandler : ICommandHandler<PayDepositCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProcessor _paymentProcessor;

    public PayDepositCommandHandler(
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        IPaymentProcessor paymentProcessor)
    {
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
        _paymentProcessor = paymentProcessor;
    }

    public async Task<Guid> HandleAsync(
        PayDepositCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            throw new NotFoundException("Reservation", command.ReservationId);

        if (reservation.Status != ReservationStatus.PendingPayment)
            throw new BadRequestException("La reserva no está pendiente de pago de seña.");

        var depositAmount = reservation.DepositAmount ?? (reservation.TotalPrice * reservation.DepositPercentage / 100);

        var paymentIntentResult = await _paymentProcessor.CreatePaymentIntentAsync(
            depositAmount,
            "ARS",
            customerId: command.UserId,
            paymentMethodId: command.PaymentMethodId,
            cancellationToken: cancellationToken);

        if (!paymentIntentResult.Success)
            throw new BadRequestException($"Error al procesar pago: {paymentIntentResult.ErrorMessage}");

        var payment = new Payment(Guid.NewGuid())
        {
            ReservationId = command.ReservationId,
            UserId = command.UserId,
            Amount = depositAmount,
            PaymentType = PaymentType.Deposit,
            PaymentMethod = command.PaymentMethodId,
            Status = PaymentStatus.Succeeded,
            PaidAt = DateTimeOffset.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        reservation.DepositAmount = depositAmount;
        reservation.DepositPaid = true;
        reservation.Status = ReservationStatus.Confirmed;

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
