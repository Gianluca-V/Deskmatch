using System.Security.Claims;
using DeskMatch.CoreService.Application.Dashboard.Dtos;
using DeskMatch.CoreService.Application.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/hosts/me/dashboard")]
[Authorize]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Obtiene el payload completo del Dashboard del Host autenticado.
    /// El HostId se extrae exclusivamente del token JWT — nunca se acepta
    /// como parámetro desde el cliente (aislamiento de datos por tenant).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var hostId = GetCurrentUserId();
        if (hostId == Guid.Empty)
            return Unauthorized();

        var dashboard = await _dashboardService.GetHostDashboardAsync(hostId, cancellationToken);
        return Ok(dashboard);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
