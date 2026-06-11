using System.Text.RegularExpressions;
using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Dtos;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Companies.Handlers;

public sealed class UpdateCompanyProfileCommandHandler
    : ICommandHandler<UpdateCompanyProfileCommand, CompanyProfileResponseDto>
{
    private readonly ICompanyRepository _repository;

    public UpdateCompanyProfileCommandHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompanyProfileResponseDto> HandleAsync(
        UpdateCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var company = await _repository.GetByOwnerIdAsync(command.OwnerId, cancellationToken)
            ?? throw new NotFoundException("Company", command.OwnerId);

        company.Name = Sanitize(command.Name);
        company.Description = string.IsNullOrWhiteSpace(command.Description)
            ? null
            : Sanitize(command.Description);
        company.ContactEmail = string.IsNullOrWhiteSpace(command.ContactEmail)
            ? null
            : command.ContactEmail.Trim();
        company.WebsiteUrl = string.IsNullOrWhiteSpace(command.WebsiteUrl)
            ? null
            : command.WebsiteUrl.Trim();
        company.PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber)
            ? null
            : command.PhoneNumber.Trim();
        company.Location = string.IsNullOrWhiteSpace(command.Location)
            ? null
            : Sanitize(command.Location);
        company.MarkAsUpdated();

        _repository.Update(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CompanyProfileResponseDto(
            company.Id,
            company.Name,
            company.Description,
            company.ContactEmail,
            company.WebsiteUrl,
            company.IsVerified,
            company.LogoUrl,
            company.PhoneNumber,
            company.Location);
    }

    private static string Sanitize(string value) =>
        Regex.Replace(value.Trim(), @"<[^>]*>", string.Empty);
}
