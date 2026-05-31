using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Dtos;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.Domain.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
[Produces("application/json")]
public sealed class CompanyController : ControllerBase
{
    private readonly ICommandHandler<CreateCompanyCommand, Guid> _createHandler;
    private readonly ICompanyRepository _repository;

    public CompanyController(
        ICommandHandler<CreateCompanyCommand, Guid> createHandler,
        ICompanyRepository repository)
    {
        _createHandler = createHandler;
        _repository = repository;
    }

    /// <summary>Crea una nueva empresa.</summary>
    /// <response code="201">Empresa creada correctamente.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyResponse>> Create(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCompanyCommand(
            request.Name,
            request.Description,
            request.LogoUrl,
            request.WebsiteUrl,
            request.OwnerId);

        var id = await _createHandler.HandleAsync(command, cancellationToken);
        var company = await _repository.GetByIdAsync(id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, ToResponse(company!));
    }

    /// <summary>Obtiene una empresa por ID.</summary>
    /// <response code="200">Empresa encontrada.</response>
    /// <response code="404">Empresa no encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(id, cancellationToken);
        if (company is null) return NotFound();
        return Ok(ToResponse(company));
    }

    private static CompanyResponse ToResponse(Domain.Companies.Company c) => new(
        c.Id, c.Name, c.Description, c.LogoUrl,
        c.WebsiteUrl, c.OwnerId, c.IsActive, c.CreatedAt);
}