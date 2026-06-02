using System.ComponentModel.DataAnnotations;

namespace DeskMatch.AuthService.Application.Auth;

public sealed record RegisterRequest(
    string? Name,
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? Role,
    string? FirstName = null,
    string? LastName = null);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record UpdateProfileRequest(
    [MaxLength(256)] string? Name,
    [MaxLength(128)] string? FirstName,
    [MaxLength(128)] string? LastName);

public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? FirstName = null,
    string? LastName = null,
    bool IsActive = true);

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserResponse User,
    string? RefreshToken = null);

public sealed record RefreshRequest([Required] string RefreshToken);
