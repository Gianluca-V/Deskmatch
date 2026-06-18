using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Dtos;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/schedules")]
[Authorize]
[Produces("application/json")]
public sealed class WorkspaceScheduleController : ControllerBase
{
    private readonly IWorkspaceScheduleRepository _repository;
    private readonly ICommandHandler<CreateWorkspaceScheduleCommand, Guid> _createHandler;
    private readonly ICommandHandler<UpdateWorkspaceScheduleCommand> _updateHandler;
    private readonly ICommandHandler<DeleteWorkspaceScheduleCommand> _deleteHandler;
    private readonly ICommandHandler<UpdateWeeklyScheduleCommand> _updateWeeklyHandler;

    public WorkspaceScheduleController(
        IWorkspaceScheduleRepository repository,
        ICommandHandler<CreateWorkspaceScheduleCommand, Guid> createHandler,
        ICommandHandler<UpdateWorkspaceScheduleCommand> updateHandler,
        ICommandHandler<DeleteWorkspaceScheduleCommand> deleteHandler,
        ICommandHandler<UpdateWeeklyScheduleCommand> updateWeeklyHandler)
    {
        _repository = repository;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _updateWeeklyHandler = updateWeeklyHandler;
    }

    /// <summary>Obtiene los horarios de un workspace.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkspaceScheduleResponse>>> GetByWorkspace(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var schedules = await _repository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        return Ok(schedules.Select(ToResponse));
    }

    /// <summary>Obtiene un horario por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkspaceScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkspaceScheduleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(id, cancellationToken);
        if (schedule is null) return NotFound();
        return Ok(ToResponse(schedule));
    }

    /// <summary>Crea un horario para un workspace.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceScheduleResponse>> Create(
        Guid workspaceId,
        [FromBody] UpdateWorkspaceScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CreateWorkspaceScheduleCommand(
            workspaceId, userId.Value, request.DayOfWeek,
            request.OpenTime, request.CloseTime, request.IsAvailable);

        var id = await _createHandler.HandleAsync(command, cancellationToken);
        var created = await _repository.GetByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { workspaceId, id }, ToResponse(created!));
    }

    /// <summary>Actualiza un horario.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkspaceScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceScheduleResponse>> Update(
        Guid workspaceId,
        Guid id,
        [FromBody] UpdateWorkspaceScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new UpdateWorkspaceScheduleCommand(
            workspaceId, id, userId.Value, request.DayOfWeek,
            request.OpenTime, request.CloseTime, request.IsAvailable);

        await _updateHandler.HandleAsync(command, cancellationToken);
        var updated = await _repository.GetByIdAsync(id, cancellationToken);
        return Ok(ToResponse(updated!));
    }

    /// <summary>Actualiza los 7 días de la semana en una sola transacción.</summary>
    [HttpPut("week")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateWeekly(
    Guid workspaceId,
    [FromBody] UpdateWeeklyScheduleRequest request,
    CancellationToken cancellationToken)
    {
    var userId = GetUserId();
    if (userId is null) return Unauthorized();

    var days = request.Days.Select(d => new DayScheduleItem(
        d.DayOfWeek, d.OpenTime, d.CloseTime, d.IsAvailable)).ToList();

    var command = new UpdateWeeklyScheduleCommand(workspaceId, userId.Value, days);
    await _updateWeeklyHandler.HandleAsync(command, cancellationToken);

    var updated = await _repository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
    return Ok(updated.Select(ToResponse));
   }

    /// <summary>Elimina un horario.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new DeleteWorkspaceScheduleCommand(workspaceId, id, userId.Value);
        await _deleteHandler.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static WorkspaceScheduleResponse ToResponse(WorkspaceSchedule s) => new(
        s.Id, s.WorkspaceId, s.DayOfWeek, s.OpenTime, s.CloseTime, s.IsAvailable);
}