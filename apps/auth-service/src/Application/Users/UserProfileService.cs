using System.Text.RegularExpressions;
using DeskMatch.AuthService.Application.Users.Dtos;
using DeskMatch.AuthService.Infrastructure.Identity;
using DeskMatch.BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DeskMatch.AuthService.Application.Users;

public sealed class UserProfileService : IUserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        return MapToDto(user);
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UserUpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        user.Name = Sanitize(dto.FullName);
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
        user.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : Sanitize(dto.Location);
        if (dto.ProfilePictureUrl is not null)
            user.ProfilePictureUrl = string.IsNullOrWhiteSpace(dto.ProfilePictureUrl) ? null : dto.ProfilePictureUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            throw new BuildingBlocks.Exceptions.ValidationException(errors);
        }

        return MapToDto(user);
    }

    private static UserProfileResponseDto MapToDto(ApplicationUser user) =>
        new(user.Id, user.Name, user.Email ?? string.Empty, user.PhoneNumber, user.Location, user.ProfilePictureUrl);

    private static string Sanitize(string value) =>
        Regex.Replace(value.Trim(), @"<[^>]*>", string.Empty);
}
