using DeskMatch.CoreService.Application.Companies.Dtos;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Companies.Commands;

public sealed record UpdateCompanyProfileCommand(
    Guid OwnerId,
    string Name,
    string? Description,
    string? ContactEmail,
    string? WebsiteUrl) : ICommand<CompanyProfileResponseDto>;
