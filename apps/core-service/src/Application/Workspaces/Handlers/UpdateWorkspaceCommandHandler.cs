using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch;
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

        var doc = CreateWorkspaceCommandHandler.ToDocument(workspace);

        if (_ollama.IsAvailable)
        {
            doc.NameVector = await _ollama.GetEmbeddingAsync(workspace.Name);
            doc.DescriptionVector = await _ollama.GetEmbeddingAsync(workspace.Description ?? "");
        }

        await _searchRepo.IndexAsync(doc, index: "offices");
    }
}
