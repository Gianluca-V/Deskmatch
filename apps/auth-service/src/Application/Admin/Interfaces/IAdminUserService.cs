using DeskMatch.AuthService.Application.Admin.Dtos;

namespace DeskMatch.AuthService.Application.Admin;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int skip, int take, CancellationToken ct = default);
    Task<AdminUserDto> ToggleUserSuspensionAsync(Guid userId, Guid adminId, string? reason, CancellationToken ct = default);
}