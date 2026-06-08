using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using OpenSearch.Client;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class CreateWorkspaceCommandHandler : ICommandHandler<CreateWorkspaceCommand, Guid>
{
    private readonly IWorkspaceRepository _repository;
    private readonly IOpenSearchRepository<WorkspaceDocument> _searchRepo;
    private readonly IOllamaClient _ollama;

    public CreateWorkspaceCommandHandler(
        IWorkspaceRepository repository,
        IOpenSearchRepository<WorkspaceDocument> searchRepo,
        IOllamaClient ollama)
    {
        _repository = repository;
        _searchRepo = searchRepo;
        _ollama = ollama;
    }

    public async Task<Guid> HandleAsync(
        CreateWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var dynamicAttributes = command.DynamicAttributes?
            .Select(a => WorkspaceAttribute.Create(a.Key, a.Value))
            .ToList() ?? [];

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
            DynamicAttributes = dynamicAttributes,
            IsActive = true
        };

        await _repository.AddAsync(workspace, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var document = MapToDocument(workspace);

        if (_ollama.IsAvailable)
        {
            document.NameVector = await _ollama.GetEmbeddingAsync(workspace.Name);
            document.DescriptionVector = await _ollama.GetEmbeddingAsync(workspace.Description ?? "");
        }

        await _searchRepo.IndexAsync(document, index: "offices");

        return workspace.Id;
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