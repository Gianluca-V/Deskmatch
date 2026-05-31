namespace DeskMatch.AuthService.Application.Auth;

public sealed record RegisterRequest(
    string? Name,
    string Email,
    string Password,
    string? Role,
    string? FirstName = null,
    string? LastName = null);

public sealed record LoginRequest(
    string Email,
    string Password);

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
    UserResponse User);
