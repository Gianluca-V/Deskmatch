namespace DeskMatch.CoreService.Application.Companies.Dtos;

public sealed record CreateCompanyRequest(
    string Name,
    string? Description,
    string? LogoUrl,
    string? WebsiteUrl,
    Guid? OwnerId);