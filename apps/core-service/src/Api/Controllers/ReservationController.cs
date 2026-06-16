using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Companies.Interfaces;
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
    private readonly ICompanyRepository _companyRepository;
    private readonly IValidator<CreateReservationRequestDto> _validator;

    public ReservationController(
        ICommandHandler<CreateReservationCommand, Guid> createHandler,
        ICommandHandler<CancelReservationCommand> cancelHandler,
        IReservationRepository reservationRepository,
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository,
        IValidator<CreateReservationRequestDto> validator)
    {
        _createHandler = createHandler;
        _cancelHandler = cancelHandler;
        _reservationRepository = reservationRepository;
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
        _validator = validator;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    private async Task<Domain.Companies.Company> GetCurrentUserCompany(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var company = await _companyRepository.GetByOwnerIdAsync(userId, cancellationToken);
        if (company is null)
            throw new NotFoundException("Company", userId);

        return company;
    }

    private async Task<Domain.Workspaces.Workspace> GetOwnedWorkspace(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            throw new NotFoundException("Workspace", workspaceId);

        var company = await GetCurrentUserCompany(cancellationToken);
        if (workspace.CompanyId != company.Id && !User.IsInRole("Admin"))
            throw new ForbiddenException("No tenés permiso para ver reservas de este espacio.");

        return workspace;
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

    /// <summary>Lista las reservas recibidas en todos los espacios de la empresa del usuario autenticado.</summary>
    /// <response code="200">Lista de reservas recibidas por la empresa.</response>
    /// <response code="404">El usuario no tiene empresa registrada.</response>
    [HttpGet("api/companies/me/reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CompanyReservationResponseDto>>> GetMyCompanyReservations(
        CancellationToken cancellationToken)
    {
        var company = await GetCurrentUserCompany(cancellationToken);
        var reservations = await _reservationRepository.GetByCompanyIdAsync(company.Id, cancellationToken);
        return Ok(await ToCompanyResponses(reservations, cancellationToken));
    }

    /// <summary>Resumen de reservas recibidas en la empresa del usuario autenticado.</summary>
    /// <response code="200">Resumen agregado de reservas recibidas.</response>
    /// <response code="404">El usuario no tiene empresa registrada.</response>
    [HttpGet("api/companies/me/reservations/summary")]
    [ProducesResponseType(typeof(CompanyReservationSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyReservationSummaryDto>> GetMyCompanyReservationsSummary(
        CancellationToken cancellationToken)
    {
        var company = await GetCurrentUserCompany(cancellationToken);
        var reservations = await _reservationRepository.GetByCompanyIdAsync(company.Id, cancellationToken);
        return Ok(ToSummary(reservations));
    }

    /// <summary>Lista las reservas recibidas por un espacio propio de la empresa.</summary>
    /// <response code="200">Lista de reservas del workspace.</response>
    /// <response code="403">El workspace no pertenece a la empresa del usuario autenticado.</response>
    /// <response code="404">Workspace no encontrado.</response>
    [HttpGet("api/workspaces/{workspaceId:guid}/reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CompanyReservationResponseDto>>> GetWorkspaceReservations(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var workspace = await GetOwnedWorkspace(workspaceId, cancellationToken);
        var reservations = await _reservationRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        return Ok(reservations.Select(r => ToCompanyResponse(r, workspace.Name)).ToList());
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

    private async Task<IReadOnlyList<CompanyReservationResponseDto>> ToCompanyResponses(
        IReadOnlyList<Domain.Reservations.Reservation> reservations,
        CancellationToken cancellationToken)
    {
        var workspaceIds = reservations.Select(r => r.WorkspaceId).Distinct().ToList();
        var workspaceNames = new Dictionary<Guid, string?>();

        foreach (var wsId in workspaceIds)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(wsId, cancellationToken);
            workspaceNames[wsId] = workspace?.Name;
        }

        return reservations
            .Select(r => ToCompanyResponse(r, workspaceNames.GetValueOrDefault(r.WorkspaceId)))
            .ToList();
    }

    private static CompanyReservationResponseDto ToCompanyResponse(
        Domain.Reservations.Reservation r,
        string? workspaceName) => new(
            r.Id,
            r.WorkspaceId,
            workspaceName,
            r.GuestId,
            r.StartTime,
            r.EndTime,
            r.TotalPrice,
            (int)r.Status,
            r.CreatedAt);

    private static CompanyReservationSummaryDto ToSummary(IReadOnlyList<Domain.Reservations.Reservation> reservations)
    {
        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var nextMonthStart = monthStart.AddMonths(1);
        var thisMonth = reservations
            .Where(r => r.StartTime >= monthStart && r.StartTime < nextMonthStart)
            .ToList();

        return new CompanyReservationSummaryDto(
            reservations.Count,
            reservations.Count(r => r.Status == Domain.Reservations.ReservationStatus.Confirmed),
            reservations.Count(r => r.Status == Domain.Reservations.ReservationStatus.Cancelled),
            reservations.Count(r => r.Status == Domain.Reservations.ReservationStatus.Completed),
            reservations.Count(r => r.Status == Domain.Reservations.ReservationStatus.Confirmed && r.StartTime > now),
            thisMonth.Count,
            thisMonth
                .Where(r => r.Status != Domain.Reservations.ReservationStatus.Cancelled)
                .Sum(r => r.TotalPrice));
    }
}
