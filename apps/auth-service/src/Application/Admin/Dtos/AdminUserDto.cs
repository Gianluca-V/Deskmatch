
namespace DeskMatch.AuthService.Application.Admin.Dtos;

public sealed record AdminUserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    bool IsSuspended,
    DateTime CreatedAt
);