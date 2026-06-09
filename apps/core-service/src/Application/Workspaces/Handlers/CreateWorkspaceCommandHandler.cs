using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class CreateWorkspaceCommandHandler : ICommandHandler<CreateWorkspaceCommand, Guid>
{
    private readonly IWorkspaceRepository _repository;

    public CreateWorkspaceCommandHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(
        CreateWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspace = new Workspace(Guid.NewGuid())
        {
            CompanyId = command.CompanyId,
            Name = command.Name,
            Description = command.Description,
            Address = command.Address,
            City = command.City,
            Country = command.Country,
            Latitude = command.Latitude,
            Longitude = command.Longitude,
            Capacity = command.Capacity,
            PricePerHour = command.PricePerHour,
            PricePerDay = command.PricePerDay,
            PricePerMonth = command.PricePerMonth,
            Amenities = command.Amenities,
            Images = command.Images,
            IsActive = true
        };

        await _repository.AddAsync(workspace, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return workspace.Id;
    }
}