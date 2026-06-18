using System.Security.Claims;
using DeskMatch.CoreService.Application.Admin.Interfaces;
using DeskMatch.CoreService.Application.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SystemAdmin")]
[Produces("application/json")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }


    [HttpGet("companies")]
    [ProducesResponseType(typeof(PagedResult<AdminCompanyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminCompanyDto>>> GetCompanies(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetCompaniesAsync(skip, take, cancellationToken);
        return Ok(result);
    }

    [HttpPut("companies/{id:guid}/toggle-verification")]
    [ProducesResponseType(typeof(AdminCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCompanyDto>> ToggleVerification(Guid id, [FromQuery] string? reason, CancellationToken cancellationToken = default)
    {
        var adminId = ExtractAdminId();
        if (adminId is null) return Unauthorized();

        var result = await _adminService.ToggleCompanyVerificationAsync(
            id, adminId.Value, reason, cancellationToken);

        return Ok(result);
    }

    private Guid? ExtractAdminId()
    {
        var sub = User.FindFirst("sub")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}