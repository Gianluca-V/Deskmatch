using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Companies.Commands;

public sealed record CreateCompanyCommand(
    string Name,
    string? Description,
    string? LogoUrl,
    string? WebsiteUrl,
    Guid OwnerId) : ICommand<Guid>;