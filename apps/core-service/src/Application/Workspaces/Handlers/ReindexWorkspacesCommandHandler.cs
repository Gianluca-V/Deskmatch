using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using OpenSearch.Client;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class ReindexWorkspacesCommandHandler : ICommandHandler<ReindexWorkspacesCommand>
{
    private readonly IWorkspaceRepository _repository;
    private readonly IOpenSearchRepository<WorkspaceDocument> _searchRepo;
    private readonly IOllamaClient _ollama;

    public ReindexWorkspacesCommandHandler(
        IWorkspaceRepository repository,
        IOpenSearchRepository<WorkspaceDocument> searchRepo,
        IOllamaClient ollama)
    {
        _repository = repository;
        _searchRepo = searchRepo;
        _ollama = ollama;
    }

    public async Task HandleAsync(
        ReindexWorkspacesCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspaces = await _repository.GetAllAsync(cancellationToken);

        var documents = new List<WorkspaceDocument>(workspaces.Count);

        foreach (var w in workspaces)
        {
            var doc = new WorkspaceDocument
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

            if (_ollama.IsAvailable)
            {
                doc = doc with
                {
                    NameVector = await _ollama.GetEmbeddingAsync(w.Name),
                    DescriptionVector = await _ollama.GetEmbeddingAsync(w.Description ?? "")
                };
            }

            documents.Add(doc);
        }

        if (documents.Count > 0)
        {
            await _searchRepo.BulkIndexAsync(documents, index: "offices");
        }
    }
}
