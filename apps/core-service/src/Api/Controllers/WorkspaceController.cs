using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Companies.Interfaces;
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
    private readonly ICommandHandler<UpdateWorkspaceCommand> _updateHandler;
    private readonly ICommandHandler<DeleteWorkspaceCommand> _deleteHandler;
    private readonly ICommandHandler<ReindexWorkspacesCommand> _reindexHandler;
    private readonly IWorkspaceRepository _repository;
    private readonly ICompanyRepository _companyRepository;

    public WorkspaceController(
        ICommandHandler<CreateWorkspaceCommand, Guid> createHandler,
        ICommandHandler<UpdateWorkspaceCommand> updateHandler,
        ICommandHandler<DeleteWorkspaceCommand> deleteHandler,
        ICommandHandler<ReindexWorkspacesCommand> reindexHandler,
        IWorkspaceRepository repository,
        ICompanyRepository companyRepository)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _reindexHandler = reindexHandler;
        _repository = repository;
        _companyRepository = companyRepository;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    private async Task ValidateCompanyOwnership(Guid companyId, CancellationToken ct)
    {
        var company = await _companyRepository.GetByIdAsync(companyId, ct);
        if (company is null)
            throw new NotFoundException("Company", companyId);
        if (company.OwnerId != GetCurrentUserId())
            throw new ForbiddenException("No tenés permiso para crear espacios a nombre de esta empresa.");
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
        await ValidateCompanyOwnership(request.CompanyId, cancellationToken);

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
            request.Images,
            request.DynamicAttributes?.Select(a => new WorkspaceAttributeInput(a.Key, a.Value)).ToList());

        var id = await _createHandler.HandleAsync(command, cancellationToken);
        var workspace = await _repository.GetByIdAsync(id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, ToResponse(workspace!));
    }

    /// <summary>Lista los workspaces de una empresa.</summary>
    /// <response code="200">Lista de workspaces de la empresa.</response>
    [HttpGet("company/{companyId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkspaceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkspaceResponse>>> GetByCompany(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var workspaces = await _repository.GetByCompanyIdAsync(companyId, cancellationToken);
        return Ok(workspaces.Select(ToResponse).ToList());
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

    /// <summary>Lista workspaces con paginación y filtros.</summary>
    /// <param name="page">Número de página (default: 1).</param>
    /// <param name="pageSize">Items por página (default: 20, max: 100).</param>
    /// <param name="city">Filtrar por ciudad.</param>
    /// <param name="country">Filtrar por país.</param>
    /// <param name="minPrice">Precio mínimo por hora.</param>
    /// <param name="maxPrice">Precio máximo por hora.</param>
    /// <param name="minCapacity">Capacidad mínima.</param>
    /// <param name="amenities">Amenities separadas por coma (WiFi,Coffee).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Lista paginada de workspaces.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<WorkspaceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<WorkspaceResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] string? amenities = null,
        CancellationToken cancellationToken = default)
    {
        // Validar y normalizar pageSize
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var (items, totalCount) = await _repository.GetPagedAsync(
            page, pageSize, city, country, minPrice, maxPrice, minCapacity, amenities, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new PagedResponse<WorkspaceResponse>(
            Items: items.Select(ToResponse).ToList(),
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPreviousPage: page > 1,
            HasNextPage: page < totalPages);

        return Ok(response);
    }

    /// <summary>Actualiza un workspace existente.</summary>
    /// <response code="200">Workspace actualizado correctamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">Workspace no encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkspaceResponse>> Update(
        Guid id,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists) return NotFound();

        await ValidateCompanyOwnership(request.CompanyId, cancellationToken);

        var command = new UpdateWorkspaceCommand(
            id,
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
            request.Images,
            request.DynamicAttributes?.Select(a => new WorkspaceAttributeInput(a.Key, a.Value)).ToList());

        await _updateHandler.HandleAsync(command, cancellationToken);

        var workspace = await _repository.GetByIdAsync(id, cancellationToken);
        return Ok(ToResponse(workspace!));
    }

    /// <summary>Elimina (soft-delete) un workspace. Admin puede eliminar cualquiera; Manager solo los suyos.</summary>
    /// <response code="204">Workspace desactivado correctamente.</response>
    /// <response code="403">Sin permiso para eliminar este workspace.</response>
    /// <response code="404">Workspace no encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workspace = await _repository.GetByIdAsync(id, cancellationToken);
        if (workspace is null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin)
            await ValidateCompanyOwnership(workspace.CompanyId, cancellationToken);

        var command = new DeleteWorkspaceCommand(id);
        await _deleteHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Reindexa todos los workspaces de PostgreSQL a OpenSearch.</summary>
    /// <response code="200">Reindexaciu00f3n completada.</response>
    [HttpPost("reindex")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reindex(CancellationToken cancellationToken)
    {
        await _reindexHandler.HandleAsync(new ReindexWorkspacesCommand(), cancellationToken);
        return Ok(new { message = "Reindex completed" });
    }

    private static WorkspaceResponse ToResponse(Domain.Workspaces.Workspace w) => new(
        w.Id, w.CompanyId, w.Name, w.Description, w.Address, w.City, w.Country,
        w.Latitude, w.Longitude, w.Capacity, w.PricePerHour, w.PricePerDay,
        w.PricePerMonth, w.Amenities, w.Images,
        w.DynamicAttributes.Select(a => new WorkspaceAttributeDto(a.Key, a.Value)).ToList(),
        w.Rating, w.ReviewCount,
        w.IsActive, w.CreatedAt);
}