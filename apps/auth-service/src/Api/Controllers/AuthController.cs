using DeskMatch.AuthService.Application.Auth;
using DeskMatch.AuthService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
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

        return Ok(_jwtTokenService.CreateToken(user, role) with { User = userResponse });
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