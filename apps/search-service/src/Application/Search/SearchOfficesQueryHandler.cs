using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace DeskMatch.SearchService.Application.Search;

public sealed class SearchOfficesQueryHandler : IQueryHandler<SearchOfficesQuery, SearchOfficesResponse>
{
    private readonly IOpenSearchRepository<WorkspaceDocument> _searchRepo;
    private readonly IOllamaClient _ollama;
    private readonly ILogger<SearchOfficesQueryHandler> _logger;

    public SearchOfficesQueryHandler(
        IOpenSearchRepository<WorkspaceDocument> searchRepo,
        IOllamaClient ollama,
        ILogger<SearchOfficesQueryHandler> logger)
    {
        _searchRepo = searchRepo;
        _ollama = ollama;
        _logger = logger;
    }

    public async Task<SearchOfficesResponse> HandleAsync(
        SearchOfficesQuery query,
        CancellationToken cancellationToken = default)
    {
        var embedding = await _ollama.GetEmbeddingAsync(query.Text ?? "");

        try
        {
            var response = await _searchRepo.SearchAsync(s => s
                .Index("offices")
                .From((query.Page - 1) * query.PageSize)
                .Size(query.PageSize)
                .Query(q => q.Bool(b => { BuildBoolQuery(query, embedding, b); return b; }))
                .Sort(sort => BuildSort(query, sort))
            );

            _logger.LogInformation(
                "Search executed: Elapsed={Elapsed}ms, IsValid={IsValid}, Total={Total}, Hits={Hits}",
                response.Took,
                response.IsValid,
                response.Total,
                response.Hits?.Count ?? 0);

            var items = response.Hits
                .Select(h => MapToResult(h.Source))
                .ToList();

            var totalPages = (int)Math.Ceiling(response.Total / (double)query.PageSize);

            return new SearchOfficesResponse(
                items.AsReadOnly(),
                query.Page,
                query.PageSize,
                response.Total,
                totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenSearch search failed");
            throw;
        }
    }

    private static void BuildBoolQuery(SearchOfficesQuery query, float[]? embedding, BoolQueryDescriptor<WorkspaceDocument> boolQuery)
    {
        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            boolQuery.Must(mu => mu.MultiMatch(mm => mm
                .Fields(f => f
                    .Field(d => d.Name, boost: 3)
                    .Field(d => d.Description, boost: 2)
                    .Field(d => d.Address))
                .Query(query.Text)
            ));
        }

        if (embedding != null)
        {
            boolQuery.Should(sh => sh
                .Knn(k => k
                    .Field(f => f.NameVector)
                    .Vector(embedding)
                    .K(10)
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(query.City))
            boolQuery.Filter(f => f.Term(t => t.Field(d => d.City).Value(query.City)));

        if (!string.IsNullOrWhiteSpace(query.Country))
            boolQuery.Filter(f => f.Term(t => t.Field(d => d.Country).Value(query.Country)));

        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
        {
            boolQuery.Filter(f => f.Range(r =>
            {
                var range = r.Field(d => d.PricePerHour);
                if (query.MinPrice.HasValue) range = range.GreaterThanOrEquals((double)query.MinPrice.Value);
                if (query.MaxPrice.HasValue) range = range.LessThanOrEquals((double)query.MaxPrice.Value);
                return range;
            }));
        }

        if (query.MinCapacity.HasValue)
            boolQuery.Filter(f => f.Range(r => r
                .Field(d => d.Capacity)
                .GreaterThanOrEquals(query.MinCapacity.Value)));

        if (query.Amenities?.Count > 0)
            boolQuery.Filter(f => f.Terms(t => t
                .Field(d => d.Amenities)
                .Terms(query.Amenities)));

        if (query.Lat.HasValue && query.Lon.HasValue)
            boolQuery.Filter(f => f.GeoDistance(g => g
                .Field(d => d.Location)
                .Distance(query.RadiusKm + "km")
                .Location(query.Lat.Value, query.Lon.Value)));
    }

    private static SortDescriptor<WorkspaceDocument> BuildSort(SearchOfficesQuery query, SortDescriptor<WorkspaceDocument> sort)
    {
        sort.Field("_score", SortOrder.Descending);
        sort.Field(f => f.Rating, SortOrder.Descending);

        if (query.Lat.HasValue && query.Lon.HasValue)
        {
            sort.GeoDistance(g => g
                .Field(d => d.Location)
                .DistanceType(GeoDistanceType.Arc)
                .Unit(DistanceUnit.Kilometers)
                .Order(SortOrder.Ascending)
                .Points(new GeoLocation(query.Lat.Value, query.Lon.Value)));
        }

        return sort;
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
