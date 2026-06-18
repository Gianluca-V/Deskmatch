using System.Security.Claims;
using DeskMatch.AuthService.Application.Admin;
using DeskMatch.AuthService.Application.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.AuthService.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SystemAdmin")]
[Produces("application/json")]
public sealed class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUserController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetUsers([FromQuery] int skip = 0,[FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var result = await _adminUserService.GetUsersAsync(skip, take, cancellationToken);
        return Ok(result);
    }

    [HttpPut("users/{id:guid}/toggle-suspension")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> ToggleSuspension(Guid id,[FromQuery] string? reason, CancellationToken cancellationToken = default)
    {
        var adminId = ExtractAdminId();
        if (adminId is null) return Unauthorized();

        var result = await _adminUserService.ToggleUserSuspensionAsync(
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