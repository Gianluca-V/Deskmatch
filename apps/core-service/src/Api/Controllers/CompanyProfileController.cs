using System.Security.Claims;
using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Dtos;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.Domain.CQRS;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
[Produces("application/json")]
public sealed class CompanyProfileController : ControllerBase
{
    private readonly ICommandHandler<UpdateCompanyProfileCommand, CompanyProfileResponseDto> _updateHandler;
    private readonly ICompanyRepository _repository;
    private readonly IValidator<CompanyUpdateProfileDto> _validator;

    public CompanyProfileController(
        ICommandHandler<UpdateCompanyProfileCommand, CompanyProfileResponseDto> updateHandler,
        ICompanyRepository repository,
        IValidator<CompanyUpdateProfileDto> validator)
    {
        _updateHandler = updateHandler;
        _repository = repository;
        _validator = validator;
    }

    /// <summary>Devuelve el perfil de la empresa del usuario autenticado.</summary>
    /// <response code="200">Perfil encontrado.</response>
    /// <response code="401">Token inválido o expirado.</response>
    /// <response code="404">El usuario no tiene empresa registrada.</response>
    [HttpGet("me/profile")]
    [ProducesResponseType(typeof(CompanyProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyProfileResponseDto>> GetMyCompanyProfile(
        CancellationToken cancellationToken)
    {
        var ownerId = ExtractUserId();
        if (ownerId is null) return Unauthorized();

        var company = await _repository.GetByOwnerIdAsync(ownerId.Value, cancellationToken);
        if (company is null) return NotFound();

        return Ok(new CompanyProfileResponseDto(
            company.Id,
            company.Name,
            company.Description,
            company.ContactEmail,
            company.WebsiteUrl,
            company.IsVerified,
            company.LogoUrl,
            company.PhoneNumber,
            company.Location));
    }

    /// <summary>Actualiza el perfil de la empresa del usuario autenticado.</summary>
    /// <remarks>
    /// IsVerified y LogoUrl no pueden modificarse desde este endpoint.
    ///
    ///     PUT /api/companies/me/profile
    ///     {
    ///         "name": "Acme Corp",
    ///         "description": "Espacios de trabajo modernos en el centro.",
    ///         "contactEmail": "contacto@acme.com",
    ///         "websiteUrl": "https://acme.com"
    ///     }
    /// </remarks>
    /// <response code="200">Perfil actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Token inválido o expirado.</response>
    /// <response code="404">El usuario no tiene empresa registrada.</response>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(CompanyProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyProfileResponseDto>> UpdateMyCompanyProfile(
        [FromBody] CompanyUpdateProfileDto dto,
        CancellationToken cancellationToken)
    {
        var ownerId = ExtractUserId();
        if (ownerId is null) return Unauthorized();

        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var command = new UpdateCompanyProfileCommand(
            ownerId.Value,
            dto.Name,
            dto.Description,
            dto.ContactEmail,
            dto.WebsiteUrl,
            dto.PhoneNumber,
            dto.Location);

        var result = await _updateHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    private Guid? ExtractUserId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
