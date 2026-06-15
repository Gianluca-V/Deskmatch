using DeskMatch.CoreService.Application.Workspaces.Dtos;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/schedules")]
[Authorize]
[Produces("application/json")]
public sealed class WorkspaceScheduleController : ControllerBase
{
    private readonly IWorkspaceScheduleRepository _repository;

    public WorkspaceScheduleController(IWorkspaceScheduleRepository repository)
    {
        _repository = repository;
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
    public async Task<ActionResult<WorkspaceScheduleResponse>> Create(
        Guid workspaceId,
        [FromBody] CreateWorkspaceScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = new WorkspaceSchedule(Guid.NewGuid())
        {
            WorkspaceId = workspaceId,
            DayOfWeek = request.DayOfWeek,
            OpenTime = request.OpenTime,
            CloseTime = request.CloseTime,
            IsAvailable = request.IsAvailable
        };

        var id = await _repository.AddAsync(schedule, cancellationToken);
        var created = await _repository.GetByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { workspaceId, id }, ToResponse(created!));
    }

    /// <summary>Elimina un horario.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(id, cancellationToken);
        if (schedule is null) return NotFound();
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static WorkspaceScheduleResponse ToResponse(WorkspaceSchedule s) => new(
        s.Id, s.WorkspaceId, s.DayOfWeek, s.OpenTime, s.CloseTime, s.IsAvailable);
}