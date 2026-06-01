using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Dtos;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
[Produces("application/json")]
public sealed class WorkspaceController : ControllerBase
{
    private readonly ICommandHandler<CreateWorkspaceCommand, Guid> _createHandler;
    private readonly IWorkspaceRepository _repository;

    public WorkspaceController(
        ICommandHandler<CreateWorkspaceCommand, Guid> createHandler,
        IWorkspaceRepository repository)
    {
        _createHandler = createHandler;
        _repository = repository;
    }

    /// <summary>Crea un nuevo workspace.</summary>
    /// <response code="201">Workspace creado correctamente.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkspaceResponse>> Create(
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkspaceCommand(
            request.CompanyId,
            request.Name,
            request.Description,
            request.Address,
            request.City,
            request.Country,
            request.Latitude,
            request.Longitude,
            request.Capacity,
            request.PricePerHour,
            request.PricePerDay,
            request.PricePerMonth,
            request.Amenities,
            request.Images);

        var id = await _createHandler.HandleAsync(command, cancellationToken);
        var workspace = await _repository.GetByIdAsync(id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, ToResponse(workspace!));
    }

    /// <summary>Obtiene un workspace por ID.</summary>
    /// <response code="200">Workspace encontrado.</response>
    /// <response code="404">Workspace no encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkspaceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workspace = await _repository.GetByIdAsync(id, cancellationToken);
        if (workspace is null) return NotFound();
        return Ok(ToResponse(workspace));
    }

    private static WorkspaceResponse ToResponse(Domain.Workspaces.Workspace w) => new(
        w.Id, w.CompanyId, w.Name, w.Description, w.Address, w.City, w.Country,
        w.Latitude, w.Longitude, w.Capacity, w.PricePerHour, w.PricePerDay,
        w.PricePerMonth, w.Amenities, w.Images, w.IsActive, w.CreatedAt);
}