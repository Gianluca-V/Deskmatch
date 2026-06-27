using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Payments.Commands;
using DeskMatch.CoreService.Application.Payments.Interfaces;
using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Domain.Payments;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Payments;

namespace DeskMatch.CoreService.Application.Payments.Handlers;

public sealed class PayFinalCommandHandler : ICommandHandler<PayFinalCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProcessor _paymentProcessor;

    public PayFinalCommandHandler(
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        IPaymentProcessor paymentProcessor)
    {
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
        _paymentProcessor = paymentProcessor;
    }

    public async Task<Guid> HandleAsync(
        PayFinalCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            throw new NotFoundException("Reservation", command.ReservationId);

        if (reservation.Status != ReservationStatus.Confirmed)
            throw new BadRequestException("La reserva debe estar confirmada para pagar el saldo final.");

        if (!reservation.DepositPaid)
            throw new BadRequestException("La seña no ha sido pagada.");

        if (reservation.FullyPaid)
            throw new BadRequestException("La reserva ya está pagada en su totalidad.");

        var depositAmount = reservation.DepositAmount ?? (reservation.TotalPrice * reservation.DepositPercentage / 100);
        var remainingAmount = reservation.TotalPrice - depositAmount;

        var paymentIntentResult = await _paymentProcessor.CreatePaymentIntentAsync(
            remainingAmount,
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
            Amount = remainingAmount,
            PaymentType = PaymentType.Final,
            PaymentMethod = command.PaymentMethodId,
            Status = PaymentStatus.Succeeded,
            PaidAt = DateTimeOffset.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        reservation.FullyPaid = true;
        reservation.Status = ReservationStatus.Completed;

        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
