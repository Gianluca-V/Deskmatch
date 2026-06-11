namespace DeskMatch.CoreService.Application.Companies.Dtos;

public sealed record CompanyUpdateProfileDto(
    string Name,
    string? Description,
    string? ContactEmail,
    string? WebsiteUrl,
    string? PhoneNumber,
    string? Location);
