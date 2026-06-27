using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Payments.Commands;
using DeskMatch.CoreService.Application.Payments.Dtos;
using DeskMatch.CoreService.Application.Payments.Interfaces;
using DeskMatch.Domain.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class PaymentController : ControllerBase
{
    private readonly ICommandHandler<PayDepositCommand, Guid> _payDepositHandler;
    private readonly ICommandHandler<PayFinalCommand, Guid> _payFinalHandler;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public PaymentController(
        ICommandHandler<PayDepositCommand, Guid> payDepositHandler,
        ICommandHandler<PayFinalCommand, Guid> payFinalHandler,
        IPaymentRepository paymentRepository,
        IPaymentMethodRepository paymentMethodRepository)
    {
        _payDepositHandler = payDepositHandler;
        _payFinalHandler = payFinalHandler;
        _paymentRepository = paymentRepository;
        _paymentMethodRepository = paymentMethodRepository;
    }

    private string GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return sub ?? throw new UnauthorizedAccessException();
    }

    /// <summary>Procesa el pago de una reserva (seña o saldo final).</summary>
    /// <response code="201">Pago procesado correctamente.</response>
    /// <response code="400">Datos de pago inválidos o reserva en estado incorrecto.</response>
    /// <response code="404">Reserva no encontrada.</response>
    [HttpPost("api/reservations/{reservationId:guid}/payments")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> PayReservation(
        Guid reservationId,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(request.PaymentType))
            throw new BadRequestException("PaymentType es requerido.");

        if (request.PaymentType.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
        {
            var command = new PayDepositCommand(reservationId, userId, request.PaymentMethodId);
            var paymentId = await _payDepositHandler.HandleAsync(command, cancellationToken);
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            return CreatedAtAction(nameof(GetPaymentMethods), ToResponse(payment!));
        }
        else if (request.PaymentType.Equals("Final", StringComparison.OrdinalIgnoreCase))
        {
            var command = new PayFinalCommand(reservationId, userId, request.PaymentMethodId);
            var paymentId = await _payFinalHandler.HandleAsync(command, cancellationToken);
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            return CreatedAtAction(nameof(GetPaymentMethods), ToResponse(payment!));
        }

        throw new BadRequestException($"PaymentType '{request.PaymentType}' no es válido. Use 'Deposit' o 'Final'.");
    }

    /// <summary>Lista los métodos de pago del usuario autenticado.</summary>
    /// <response code="200">Lista de métodos de pago guardados.</response>
    [HttpGet("api/payments/methods")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentMethodResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentMethodResponse>>> GetPaymentMethods(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var methods = await _paymentMethodRepository.GetByUserIdAsync(userId, cancellationToken);
        return Ok(methods.Select(m => new PaymentMethodResponse(
            m.Id,
            m.LastFourDigits,
            m.ExpiryMonth,
            m.ExpiryYear,
            m.CardHolderName,
            m.IsDefault,
            m.CreatedAt)).ToList());
    }

    /// <summary>Guarda un nuevo método de pago para el usuario autenticado.</summary>
    /// <response code="201">Método de pago guardado correctamente.</response>
    /// <response code="400">Datos de método de pago inválidos.</response>
    [HttpPost("api/payments/methods")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentMethodResponse>> SavePaymentMethod(
        [FromBody] PaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(request.LastFourDigits) ||
            request.ExpiryMonth <= 0 || request.ExpiryMonth > 12 ||
            request.ExpiryYear <= 0 || string.IsNullOrWhiteSpace(request.CardHolderName))
            throw new BadRequestException("Datos de método de pago inválidos.");

        var method = new DeskMatch.CoreService.Domain.Payments.PaymentMethod(Guid.NewGuid())
        {
            UserId = userId,
            LastFourDigits = request.LastFourDigits,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            CardHolderName = request.CardHolderName,
            IsDefault = request.IsDefault
        };

        await _paymentMethodRepository.AddAsync(method, cancellationToken);
        await _paymentMethodRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPaymentMethods), new PaymentMethodResponse(
            method.Id,
            method.LastFourDigits,
            method.ExpiryMonth,
            method.ExpiryYear,
            method.CardHolderName,
            method.IsDefault,
            method.CreatedAt));
    }

    /// <summary>Elimina un método de pago guardado del usuario autenticado.</summary>
    /// <response code="200">Método de pago eliminado correctamente.</response>
    /// <response code="403">El método de pago no pertenece al usuario autenticado.</response>
    /// <response code="404">Método de pago no encontrado.</response>
    [HttpDelete("api/payments/methods/{methodId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePaymentMethod(
        Guid methodId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var method = await _paymentMethodRepository.GetByIdAsync(methodId, cancellationToken);

        if (method is null)
            throw new NotFoundException("PaymentMethod", methodId);

        if (method.UserId != userId)
            throw new ForbiddenException("No tenés permiso para eliminar este método de pago.");

        await _paymentMethodRepository.DeleteAsync(methodId, cancellationToken);
        await _paymentMethodRepository.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    private static PaymentResponse ToResponse(DeskMatch.CoreService.Domain.Payments.Payment p) => new(
        p.Id,
        p.ReservationId,
        p.Amount,
        (int)p.PaymentType,
        (int)p.Status,
        p.PaidAt,
        p.CreatedAt);
}
