namespace DeskMatch.AuthService.Application.Users.Dtos;

public sealed record UserUpdateProfileDto(
    string FullName,
    string? PhoneNumber,
    string? Location);
