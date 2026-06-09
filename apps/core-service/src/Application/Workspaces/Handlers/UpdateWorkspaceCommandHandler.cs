using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class UpdateWorkspaceCommandHandler : ICommandHandler<UpdateWorkspaceCommand>
{
    private readonly IWorkspaceRepository _repository;

    public UpdateWorkspaceCommandHandler(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        UpdateWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _repository.GetByIdAsync(command.Id, cancellationToken);

        workspace!.CompanyId = command.CompanyId;
        workspace.Name = command.Name;
        workspace.Description = command.Description;
        workspace.Address = command.Address;
        workspace.City = command.City;
        workspace.Country = command.Country;
        workspace.Latitude = command.Latitude;
        workspace.Longitude = command.Longitude;
        workspace.Capacity = command.Capacity;
        workspace.PricePerHour = command.PricePerHour;
        workspace.PricePerDay = command.PricePerDay;
        workspace.PricePerMonth = command.PricePerMonth;
        workspace.Amenities = command.Amenities;
        workspace.Images = command.Images;
        workspace.MarkAsUpdated();

        _repository.Update(workspace);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
