using DeskMatch.AuthService.Application.Users.Dtos;

namespace DeskMatch.AuthService.Application.Users;

public interface IUserProfileService
{
    Task<UserProfileResponseDto> GetProfileAsync(Guid userId);
    Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UserUpdateProfileDto dto);
}
