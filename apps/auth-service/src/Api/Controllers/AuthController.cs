using DeskMatch.AuthService.Application.Auth;
using DeskMatch.AuthService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DeskMatch.AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenBlacklistService _blacklist;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ITokenBlacklistService blacklist,
        IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _blacklist = blacklist;
        _refreshTokenService = refreshTokenService;
    }

    /// <summary>Registra un nuevo usuario.</summary>
    /// <remarks>
    /// Ejemplo:
    ///
    ///     POST /api/auth/register
    ///     {
    ///         "name": "Juan Pérez",
    ///         "email": "juan@empresa.com",
    ///         "password": "MiPassword1!",
    ///         "role": "User",
    ///         "firstName": "Juan",
    ///         "lastName": "Pérez"
    ///     }
    ///
    /// Roles válidos: Admin, Manager, User. Si no se envía role, se asigna User por defecto.
    /// </remarks>
    /// <response code="201">Usuario creado correctamente.</response>
    /// <response code="400">Datos inválidos o contraseña que no cumple los requisitos.</response>
    /// <response code="409">El email ya está registrado.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        var role = string.IsNullOrWhiteSpace(request.Role) ? AuthRoles.User : request.Role.Trim();

        if (!AuthRoles.All.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["role"] = [$"Role must be one of: {string.Join(", ", AuthRoles.All)}."]
            }));
        }

        role = AuthRoles.All.First(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            var firstName = request.FirstName?.Trim() ?? string.Empty;
            var lastName = request.LastName?.Trim() ?? string.Empty;
            name = $"{firstName} {lastName}".Trim();
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return IdentityValidationProblem(createResult);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return IdentityValidationProblem(roleResult);
        }

        return CreatedAtAction(nameof(Me), new UserResponse(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            role,
            user.FirstName,
            user.LastName,
            user.IsActive));
    }

    /// <summary>Inicia sesión y devuelve un JWT.</summary>
    /// <remarks>
    /// Ejemplo:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "email": "juan@empresa.com",
    ///         "password": "MiPassword1!"
    ///     }
    ///
    /// Usar el token recibido en requests protegidas:
    ///
    ///     Authorization: Bearer {accessToken}
    ///
    /// </remarks>
    /// <response code="200">Login exitoso, devuelve token y datos del usuario.</response>
    /// <response code="401">Credenciales inválidas o usuario inactivo.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "User account is inactive." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? AuthRoles.User;
        var userResponse = new UserResponse(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            role,
            user.FirstName,
            user.LastName,
            user.IsActive);

        var loginResponse = _jwtTokenService.CreateToken(user, role) with { User = userResponse };
        var refreshToken = await _refreshTokenService.GenerateAsync(user);

        return Ok(loginResponse with { RefreshToken = refreshToken.Token });
    }

    /// <summary>Devuelve los datos del usuario autenticado.</summary>
    /// <remarks>Requiere header: Authorization: Bearer {token}</remarks>
    /// <response code="200">Datos del usuario autenticado.</response>
    /// <response code="401">Token inválido o expirado.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? AuthRoles.User;
        return Ok(new UserResponse(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            role,
            user.FirstName,
            user.LastName,
            user.IsActive));
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> UpdateMe(UpdateProfileRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var nameRecalcNeeded = false;

        if (request.FirstName is not null)
        {
            user.FirstName = request.FirstName.Trim();
            nameRecalcNeeded = true;
        }

        if (request.LastName is not null)
        {
            user.LastName = request.LastName.Trim();
            nameRecalcNeeded = true;
        }

        if (request.Name is not null)
            user.Name = request.Name.Trim();
        else if (nameRecalcNeeded)
            user.Name = $"{user.FirstName} {user.LastName}".Trim();

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return IdentityValidationProblem(result);
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? AuthRoles.User;
        return Ok(new UserResponse(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            role,
            user.FirstName,
            user.LastName,
            user.IsActive));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Refresh(RefreshRequest request)
    {
        var existing = await _refreshTokenService.FindValidAsync(request.RefreshToken);
        if (existing is null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        if (!existing.User.IsActive)
            return Unauthorized(new { message = "User account is inactive." });

        await _refreshTokenService.MarkUsedAsync(existing);

        var role = (await _userManager.GetRolesAsync(existing.User)).FirstOrDefault() ?? AuthRoles.User;
        var userResponse = new UserResponse(
            existing.User.Id,
            existing.User.Name,
            existing.User.Email ?? string.Empty,
            role,
            existing.User.FirstName,
            existing.User.LastName,
            existing.User.IsActive);

        var loginResponse = _jwtTokenService.CreateToken(existing.User, role) with { User = userResponse };
        var newRefreshToken = await _refreshTokenService.GenerateAsync(existing.User);

        return Ok(loginResponse with { RefreshToken = newRefreshToken.Token });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti))
            return NoContent();

        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        var expiresIn = TimeSpan.FromMinutes(60);

        if (long.TryParse(expClaim, out var expUnix))
        {
            var remaining = DateTimeOffset.FromUnixTimeSeconds(expUnix) - DateTimeOffset.UtcNow;
            if (remaining > TimeSpan.Zero)
                expiresIn = remaining;
        }

        await _blacklist.BlacklistAsync(jti, expiresIn);
        return NoContent();
    }

    private ActionResult IdentityValidationProblem(IdentityResult result)
    {
        var errors = result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return BadRequest(new ValidationProblemDetails(errors));
    }
}