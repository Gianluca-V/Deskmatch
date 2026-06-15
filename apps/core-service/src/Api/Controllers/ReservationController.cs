using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Reservations.Commands;
using DeskMatch.CoreService.Application.Reservations.Dtos;
using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class ReservationController : ControllerBase
{
    private readonly ICommandHandler<CreateReservationCommand, Guid> _createHandler;
    private readonly ICommandHandler<CancelReservationCommand> _cancelHandler;
    private readonly IReservationRepository _reservationRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IValidator<CreateReservationRequestDto> _validator;

    public ReservationController(
        ICommandHandler<CreateReservationCommand, Guid> createHandler,
        ICommandHandler<CancelReservationCommand> cancelHandler,
        IReservationRepository reservationRepository,
        IWorkspaceRepository workspaceRepository,
        IValidator<CreateReservationRequestDto> validator)
    {
        _createHandler = createHandler;
        _cancelHandler = cancelHandler;
        _reservationRepository = reservationRepository;
        _workspaceRepository = workspaceRepository;
        _validator = validator;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    /// <summary>Crea una reserva para un workspace.</summary>
    /// <response code="201">Reserva creada correctamente.</response>
    /// <response code="400">Fechas inválidas o en el pasado.</response>
    /// <response code="403">El usuario intenta reservar su propio espacio.</response>
    /// <response code="404">Workspace no encontrado.</response>
    /// <response code="409">Solapamiento con una reserva existente o espacio cerrado.</response>
    [HttpPost("api/workspaces/{workspaceId:guid}/reservations")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> Create(
        Guid workspaceId,
        [FromBody] CreateReservationRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new DeskMatch.BuildingBlocks.Exceptions.ValidationException(errors);
        }

        var guestId = GetCurrentUserId();
        var command = new CreateReservationCommand(workspaceId, guestId, request.StartTime, request.EndTime);
        var reservationId = await _createHandler.HandleAsync(command, cancellationToken);

        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

        return CreatedAtAction(
            nameof(GetMyReservations),
            ToResponse(reservation!));
    }

    /// <summary>Cancela una reserva propia confirmada y futura.</summary>
    /// <response code="200">Reserva cancelada correctamente.</response>
    /// <response code="400">La reserva ya pasó o no está confirmada.</response>
    /// <response code="403">La reserva no pertenece al usuario.</response>
    /// <response code="404">Reserva no encontrada.</response>
    [HttpPost("api/reservations/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var guestId = GetCurrentUserId();
        var command = new CancelReservationCommand(id, guestId);
        await _cancelHandler.HandleAsync(command, cancellationToken);
        return Ok();
    }

    /// <summary>Lista todas las reservas del usuario autenticado.</summary>
    /// <response code="200">Lista de reservas del guest.</response>
    [HttpGet("api/reservations/me")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponseDto>>> GetMyReservations(
        CancellationToken cancellationToken)
    {
        var guestId = GetCurrentUserId();
        var reservations = await _reservationRepository.GetByGuestIdAsync(guestId, cancellationToken);

        var workspaceIds = reservations.Select(r => r.WorkspaceId).Distinct().ToList();
        var workspaceNames = new Dictionary<Guid, string?>();
        foreach (var wsId in workspaceIds)
        {
            var ws = await _workspaceRepository.GetByIdAsync(wsId, cancellationToken);
            workspaceNames[wsId] = ws?.Name;
        }

        return Ok(reservations.Select(r => ToResponse(r, workspaceNames.GetValueOrDefault(r.WorkspaceId))).ToList());
    }

    private static ReservationResponseDto ToResponse(Domain.Reservations.Reservation r, string? workspaceName = null) => new(
        r.Id,
        r.WorkspaceId,
        r.GuestId,
        r.StartTime,
        r.EndTime,
        r.TotalPrice,
        (int)r.Status,
        r.CreatedAt,
        workspaceName);
}
