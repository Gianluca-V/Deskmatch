namespace DeskMatch.CoreService.Application.Companies.Dtos;

public sealed record CompanyProfileResponseDto(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? WebsiteUrl,
    bool IsVerified,
    string? LogoUrl,
    string? PhoneNumber,
    string? Location,
    string? OwnerEmail = null);
