using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Companies.Handlers;

public sealed class CreateCompanyCommandHandler : ICommandHandler<CreateCompanyCommand, Guid>
{
    private readonly ICompanyRepository _repository;

    public CreateCompanyCommandHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(
        CreateCompanyCommand command,
        CancellationToken cancellationToken = default)
    {
      var company = new Company(Guid.NewGuid())
      {
          Name = command.Name,
          Description = command.Description,
          LogoUrl = command.LogoUrl,
          WebsiteUrl = command.WebsiteUrl,
          OwnerId = command.OwnerId,
          IsActive = true
      };
        await _repository.AddAsync(company, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return company.Id;
    }
}