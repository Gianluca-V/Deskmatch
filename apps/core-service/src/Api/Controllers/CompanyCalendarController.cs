using System.Security.Claims;
using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.CompanyCalendar.Dtos;
using DeskMatch.CoreService.Application.CompanyCalendar.Interfaces;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/companies/me/calendar")]
[Authorize(Roles = "Manager")]
[Produces("application/json")]
public sealed class CompanyCalendarController : ControllerBase
{
    private readonly ICompanyCalendarRepository _calendarRepository;
    private readonly ICompanyRepository _companyRepository;

    public CompanyCalendarController(
        ICompanyCalendarRepository calendarRepository,
        ICompanyRepository companyRepository)
    {
        _calendarRepository = calendarRepository;
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Obtiene el calendario mensual de reservas de la empresa del usuario autenticado.
    /// El CompanyId se extrae exclusivamente del JWT — nunca se acepta como parámetro
    /// desde el cliente (aislamiento de datos por tenant).
    /// </summary>
    /// <param name="year">Año del calendario (ej. 2026).</param>
    /// <param name="month">Mes del calendario (1-12).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Lista de reservas del mes solicitado.</response>
    /// <response code="400">Parámetros inválidos (año o mes fuera de rango).</response>
    /// <response code="401">Usuario no autenticado.</response>
    /// <response code="403">El usuario no tiene el rol Manager.</response>
    /// <response code="404">El usuario no tiene una empresa registrada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyCalendarEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CompanyCalendarEntryDto>>> GetCalendar(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return BadRequest(new ProblemDetails
            {
                Title = "Parámetros inválidos",
                Detail = $"El mes debe estar entre 1 y 12. Valor recibido: {month}.",
                Status = StatusCodes.Status400BadRequest
            });

        if (year < 2020 || year > 2100)
            return BadRequest(new ProblemDetails
            {
                Title = "Parámetros inválidos",
                Detail = $"El año debe estar entre 2020 y 2100. Valor recibido: {year}.",
                Status = StatusCodes.Status400BadRequest
            });

        var company = await GetCurrentUserCompany(cancellationToken);

        var monthStart = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var entries = await _calendarRepository.GetReservationsInRangeAsync(
            company.Id, monthStart, monthEnd, cancellationToken);

        return Ok(entries);
    }

    private async Task<Domain.Companies.Company> GetCurrentUserCompany(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(sub, out var userId) || userId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var company = await _companyRepository.GetByOwnerIdAsync(userId, ct);
        if (company is null)
            throw new NotFoundException("Company", userId);

        return company;
    }
}
