using System.Text;
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
            .Query(q => q.Bool(b =>
            {
                if (!string.IsNullOrWhiteSpace(query.Text))
                {
                    b.Must(mu => mu.MultiMatch(mm => mm
                        .Fields(new[] { "name^3", "description^2", "address" })
                        .Query(query.Text)));
                }

                if (!string.IsNullOrWhiteSpace(query.City))
                    b.Filter(f => f.Term("city", query.City));
                if (!string.IsNullOrWhiteSpace(query.Country))
                    b.Filter(f => f.Term("country", query.Country));

                if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
                    b.Filter(f => f.Range(r =>
                    {
                        r.Field("pricePerHour");
                        if (query.MinPrice.HasValue) r.GreaterThanOrEquals((double)query.MinPrice.Value);
                        if (query.MaxPrice.HasValue) r.LessThanOrEquals((double)query.MaxPrice.Value);
                        return r;
                    }));

                if (query.MinCapacity.HasValue)
                    b.Filter(f => f.Range(r => r.Field("capacity").GreaterThanOrEquals(query.MinCapacity.Value)));

                if (query.Amenities?.Count > 0)
                    b.Filter(f => f.Terms(t => t.Field("amenities").Terms(query.Amenities)));

                if (query.Lat.HasValue && query.Lon.HasValue)
                    b.Filter(f => f.GeoDistance(g => g
                        .Field("location")
                        .Distance((query.RadiusKm ?? 10) + "km")
                        .Location(query.Lat.Value, query.Lon.Value)));

                return b;
            }))
            .Sort(sort =>
            {
                sort.Field("_score", SortOrder.Descending);
                sort.Field("rating", SortOrder.Descending);
                if (query.Lat.HasValue && query.Lon.HasValue)
                {
                    sort.GeoDistance(g => g
                        .Field("location")
                        .DistanceType(GeoDistanceType.Arc)
                        .Unit(DistanceUnit.Kilometers)
                        .Order(SortOrder.Ascending)
                        .Points(new GeoLocation(query.Lat.Value, query.Lon.Value)));
                }
                return sort;
            })
        );

        return ParseResponse(response, query.Page, query.PageSize);
    }

    private SearchOfficesResponse ParseResponse(ISearchResponse<object> response, int page, int pageSize)
    {
        var rawBody = response.ApiCall?.ResponseBodyInBytes;
        if (rawBody == null || rawBody.Length == 0)
            return new SearchOfficesResponse(new List<OfficeResult>().AsReadOnly(), page, pageSize, 0, 0);

        var root = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(rawBody));
        var hits = root.GetProperty("hits");
        var totalObj = hits.GetProperty("total");

        long total = totalObj.ValueKind == JsonValueKind.Object
            ? totalObj.GetProperty("value").GetInt64()
            : totalObj.GetInt64();

        var hitsArray = hits.GetProperty("hits");
        var items = new List<OfficeResult>();

        foreach (var hit in hitsArray.EnumerateArray())
        {
            var source = hit.GetProperty("_source");
            var doc = JsonSerializer.Deserialize<WorkspaceDocument>(source.GetRawText(), JsonOptions);
            if (doc != null)
                items.Add(MapToResult(doc));
        }

        var totalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return new SearchOfficesResponse(items.AsReadOnly(), page, pageSize, total, totalPages);
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
