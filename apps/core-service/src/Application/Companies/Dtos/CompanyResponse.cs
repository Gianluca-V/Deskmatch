namespace DeskMatch.CoreService.Application.Companies.Dtos;

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string? WebsiteUrl,
    Guid? OwnerId,
    bool IsActive,
    DateTime CreatedAt);