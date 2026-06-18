using DeskMatch.AuthService.Application.Admin.Dtos;
using DeskMatch.AuthService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.AuthService.Application.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(int skip, int take, CancellationToken ct = default)
    {
        var total = await _userManager.Users.CountAsync(ct);
        var users = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        var items = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new AdminUserDto(
                user.Id,
                user.Name,
                user.Email ?? string.Empty,
                roles.FirstOrDefault() ?? AuthRoles.User,
                user.IsActive,
                user.IsSuspended,
                user.CreatedAt));
        }

        return new PagedResult<AdminUserDto>(items, total, skip, take);
    }

    public async Task<AdminUserDto> ToggleUserSuspensionAsync(Guid userId, Guid adminId, string? reason, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.IsSuspended = !user.IsSuspended;
        user.IsActive = !user.IsSuspended;

        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        return new AdminUserDto(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            roles.FirstOrDefault() ?? AuthRoles.User,
            user.IsActive,
            user.IsSuspended,
            user.CreatedAt);
    }
}