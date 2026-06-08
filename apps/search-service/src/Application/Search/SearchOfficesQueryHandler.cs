using System.Text.Json;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace DeskMatch.SearchService.Application.Search;

public sealed class SearchOfficesQueryHandler : IQueryHandler<SearchOfficesQuery, SearchOfficesResponse>
{
    private readonly IOpenSearchClient _client;
    private readonly IOllamaClient _ollama;
    private readonly ILogger<SearchOfficesQueryHandler> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SearchOfficesQueryHandler(
        IOpenSearchClient client,
        IOllamaClient ollama,
        ILogger<SearchOfficesQueryHandler> logger)
    {
        _client = client;
        _ollama = ollama;
        _logger = logger;
    }

    public async Task<SearchOfficesResponse> HandleAsync(
        SearchOfficesQuery query,
        CancellationToken cancellationToken = default)
    {
        var embedding = await _ollama.GetEmbeddingAsync(query.Text ?? "");

        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .From((query.Page - 1) * query.PageSize)
            .Size(query.PageSize)
            .Query(q => q.MatchAll())
        );

        _logger.LogInformation(
            "DEBUG handler: IsValid={IsValid}, Total={Total}, Hits={Hits}, Raw json={Response}",
            response.IsValid,
            response.Total,
            response.Hits?.Count ?? 0,
            response.DebugInformation ?? "no debug");

        var items = new List<OfficeResult>();
        if (response.Hits != null)
        {
            foreach (var hit in response.Hits)
            {
                if (hit.Source is JsonElement el)
                {
                    var doc = JsonSerializer.Deserialize<WorkspaceDocument>(el.GetRawText(), JsonOptions);
                    if (doc != null)
                        items.Add(MapToResult(doc));
                }
            }
        }

        var totalPages = response.Total > 0
            ? (int)Math.Ceiling(response.Total / (double)query.PageSize)
            : 0;

        return new SearchOfficesResponse(
            items.AsReadOnly(),
            query.Page,
            query.PageSize,
            response.Total > 0 ? response.Total : response.Hits?.Count ?? 0,
            totalPages);
    }

    private static OfficeResult MapToResult(WorkspaceDocument doc) => new(
        doc.Id,
        doc.Name,
        doc.Description,
        doc.City,
        doc.Country,
        doc.Address,
        doc.Capacity,
        doc.PricePerHour,
        doc.Amenities,
        doc.DynamicAttributes,
        doc.Location?.Latitude,
        doc.Location?.Longitude,
        doc.Rating,
        doc.ReviewCount);
}
