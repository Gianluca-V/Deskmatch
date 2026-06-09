namespace DeskMatch.AuthService.Application.Users.Dtos;

public sealed record UserProfileResponseDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? Location);
