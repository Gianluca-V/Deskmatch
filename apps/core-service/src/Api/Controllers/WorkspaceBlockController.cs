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
[Route("api/workspaces/{workspaceId:guid}/blocks")]
[Authorize]
[Produces("application/json")]
public sealed class WorkspaceBlockController : ControllerBase
{
    private readonly IWorkspaceBlockRepository _repository;
    private readonly ICommandHandler<CreateWorkspaceBlockCommand, Guid> _createHandler;
    private readonly ICommandHandler<DeleteWorkspaceBlockCommand> _deleteHandler;

    public WorkspaceBlockController(
        IWorkspaceBlockRepository repository,
        ICommandHandler<CreateWorkspaceBlockCommand, Guid> createHandler,
        ICommandHandler<DeleteWorkspaceBlockCommand> deleteHandler)
    {
        _repository = repository;
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>Obtiene los bloqueos de un workspace.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceBlockResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkspaceBlockResponse>>> GetByWorkspace(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var blocks = await _repository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        return Ok(blocks.Select(ToResponse));
    }

    /// <summary>Crea un bloqueo para un workspace.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceBlockResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceBlockResponse>> Create(
        Guid workspaceId,
        [FromBody] CreateWorkspaceBlockRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CreateWorkspaceBlockCommand(
            workspaceId, userId.Value, request.BlockStart, request.BlockEnd, request.Reason);

        var id = await _createHandler.HandleAsync(command, cancellationToken);
        var created = await _repository.GetByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetByWorkspace), new { workspaceId }, ToResponse(created!));
    }

    /// <summary>Elimina un bloqueo.</summary>
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

        var command = new DeleteWorkspaceBlockCommand(workspaceId, id, userId.Value);
        await _deleteHandler.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static WorkspaceBlockResponse ToResponse(WorkspaceBlock b) => new(
        b.Id, b.WorkspaceId, b.BlockStart, b.BlockEnd, b.Reason);
}