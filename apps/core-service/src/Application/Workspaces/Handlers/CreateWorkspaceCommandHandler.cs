using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch;
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

        await IndexToOpenSearchAsync(workspace, cancellationToken);

        return workspace.Id;
    }

    private async Task IndexToOpenSearchAsync(Workspace workspace, CancellationToken ct)
    {
        var doc = ToDocument(workspace);

        if (_ollama.IsAvailable)
        {
            doc.NameVector = await _ollama.GetEmbeddingAsync(workspace.Name);
            doc.DescriptionVector = await _ollama.GetEmbeddingAsync(workspace.Description ?? "");
        }

        await _searchRepo.IndexAsync(doc, index: "offices");
    }

    internal static WorkspaceDocument ToDocument(Workspace w) => new()
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
        Images = w.Images,
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
