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
    private readonly IOpenSearchClient _client;
    private readonly IOllamaClient _ollama;

    public ReindexWorkspacesCommandHandler(
        IWorkspaceRepository repository,
        IOpenSearchRepository<WorkspaceDocument> searchRepo,
        IOpenSearchClient client,
        IOllamaClient ollama)
    {
        _repository = repository;
        _searchRepo = searchRepo;
        _client = client;
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
            if (w.IsActive is false) continue;

            var doc = CreateWorkspaceCommandHandler.ToDocument(w);

            if (_ollama.IsAvailable)
            {
                doc.NameVector = await _ollama.GetEmbeddingAsync(w.Name);
                doc.DescriptionVector = await _ollama.GetEmbeddingAsync(w.Description ?? "");
            }

            documents.Add(doc);
        }

        await _client.DeleteByQueryAsync<WorkspaceDocument>(d => d
            .Index("offices")
            .Query(q => q.MatchAll()));

        if (documents.Count > 0)
        {
            await _searchRepo.BulkIndexAsync(documents, index: "offices");
        }
    }
}
