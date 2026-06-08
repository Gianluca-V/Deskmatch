using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using OpenSearch.Client;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class UpdateWorkspaceCommandHandler : ICommandHandler<UpdateWorkspaceCommand>
{
    private readonly IWorkspaceRepository _repository;
    private readonly IOpenSearchRepository<WorkspaceDocument> _searchRepo;
    private readonly IOllamaClient _ollama;

    public UpdateWorkspaceCommandHandler(
        IWorkspaceRepository repository,
        IOpenSearchRepository<WorkspaceDocument> searchRepo,
        IOllamaClient ollama)
    {
        _repository = repository;
        _searchRepo = searchRepo;
        _ollama = ollama;
    }

    public async Task HandleAsync(
        UpdateWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _repository.GetByIdAsync(command.Id, cancellationToken);

        var dynamicAttributes = command.DynamicAttributes?
            .Select(a => WorkspaceAttribute.Create(a.Key, a.Value))
            .ToList() ?? [];

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
        workspace.DynamicAttributes = dynamicAttributes;
        workspace.MarkAsUpdated();

        _repository.Update(workspace);
        await _repository.SaveChangesAsync(cancellationToken);

        var document = MapToDocument(workspace);

        if (_ollama.IsAvailable)
        {
            document.NameVector = await _ollama.GetEmbeddingAsync(workspace.Name);
            document.DescriptionVector = await _ollama.GetEmbeddingAsync(workspace.Description ?? "");
        }

        await _searchRepo.UpdateAsync(workspace.Id.ToString(), document, index: "offices");
    }

    private static WorkspaceDocument MapToDocument(Workspace w) => new()
    {
        Id = w.Id.ToString(),
        Name = w.Name,
        Description = w.Description,
        City = w.City,
        Country = w.Country,
        Address = w.Address,
        Capacity = w.Capacity,
        PricePerHour = (double)w.PricePerHour,
        Amenities = w.Amenities,
        Location = w.Latitude.HasValue && w.Longitude.HasValue
            ? new GeoLocation(w.Latitude.Value, w.Longitude.Value)
            : null,
        Rating = w.Rating,
        ReviewCount = w.ReviewCount,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        DynamicAttributes = w.DynamicAttributes?
            .ToDictionary(a => a.Key, a => (object)(a.Value ?? ""))
    };
}
