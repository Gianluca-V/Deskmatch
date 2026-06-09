using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DeskMatch.AuthService.Application.Users;
using DeskMatch.AuthService.Application.Users.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.AuthService.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public sealed class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly IValidator<UserUpdateProfileDto> _validator;

    public UserProfileController(
        IUserProfileService userProfileService,
        IValidator<UserUpdateProfileDto> validator)
    {
        _userProfileService = userProfileService;
        _validator = validator;
    }

    /// <summary>Devuelve el perfil del usuario autenticado.</summary>
    /// <response code="200">Perfil encontrado.</response>
    /// <response code="401">Token inválido o expirado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [HttpGet("me/profile")]
    [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponseDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized();

        var profile = await _userProfileService.GetProfileAsync(userId.Value);
        return Ok(profile);
    }

    /// <summary>Actualiza el perfil del usuario autenticado.</summary>
    /// <remarks>
    /// No se puede modificar el email.
    /// PhoneNumber vacío persiste como null.
    ///
    ///     PUT /api/users/me/profile
    ///     {
    ///         "fullName": "Juan Pérez",
    ///         "phoneNumber": "+5491112345678",
    ///         "location": "Buenos Aires, Argentina"
    ///     }
    /// </remarks>
    /// <response code="200">Perfil actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Token inválido o expirado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponseDto>> UpdateMyProfile(
        [FromBody] UserUpdateProfileDto dto,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized();

        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var profile = await _userProfileService.UpdateProfileAsync(userId.Value, dto);
        return Ok(profile);
    }

    private Guid? ExtractUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
