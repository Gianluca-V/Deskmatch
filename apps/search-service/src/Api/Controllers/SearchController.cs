using System.Text.Json;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;

namespace DeskMatch.SearchService.Api.Controllers;

[ApiController]
[Route("api/search")]
[Produces("application/json")]
public sealed class SearchController : ControllerBase
{
    private readonly IOpenSearchClient _client;
    private readonly IOllamaClient _ollama;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SearchController(IOpenSearchClient client, IOllamaClient ollama)
    {
        _client = client;
        _ollama = ollama;
    }

    [HttpGet("offices")]
    public async Task<IActionResult> SearchOffices(
        [FromQuery] string? q,
        [FromQuery] string? city,
        [FromQuery] string? country,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minCapacity,
        [FromQuery] string? amenities,
        [FromQuery] double? lat,
        [FromQuery] double? lon,
        [FromQuery] double? radius,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var embedding = await _ollama.GetEmbeddingAsync(q ?? "");

        var hasQuery = !string.IsNullOrWhiteSpace(q);
        var hasFilters = !string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(country)
            || minPrice.HasValue || maxPrice.HasValue || minCapacity.HasValue
            || !string.IsNullOrWhiteSpace(amenities) || (lat.HasValue && lon.HasValue);

        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(qq =>
            {
                if (!hasQuery && !hasFilters)
                    return qq.MatchAll();

                return qq.Bool(b =>
                {
                if (!string.IsNullOrWhiteSpace(q))
                    b.Must(mu => mu.MultiMatch(mm => mm
                        .Fields(new[] { "name^3", "description^2", "address" })
                        .Query(q)));

                if (!string.IsNullOrWhiteSpace(city))
                    b.Filter(f => f.Term("city", city));
                if (!string.IsNullOrWhiteSpace(country))
                    b.Filter(f => f.Term("country", country));

                if (minPrice.HasValue || maxPrice.HasValue)
                    b.Filter(f => f.Range(r =>
                    {
                        r.Field("pricePerHour");
                        if (minPrice.HasValue) r.GreaterThanOrEquals((double)minPrice.Value);
                        if (maxPrice.HasValue) r.LessThanOrEquals((double)maxPrice.Value);
                        return r;
                    }));

                if (minCapacity.HasValue)
                    b.Filter(f => f.Range(r => r.Field("capacity").GreaterThanOrEquals(minCapacity.Value)));

                var amList = amenities?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (amList?.Length > 0)
                    b.Filter(f => f.Terms(t => t.Field("amenities").Terms(amList)));

                if (lat.HasValue && lon.HasValue)
                    b.Filter(f => f.GeoDistance(g => g
                        .Field("location")
                        .Distance((radius ?? 10) + "km")
                        .Location(lat.Value, lon.Value)));

                return b;
            });
            })
            .Sort(sort =>
            {
                sort.Field("_score", SortOrder.Descending);
                sort.Field("rating", SortOrder.Descending);
                if (lat.HasValue && lon.HasValue)
                    sort.GeoDistance(g => g
                        .Field("location")
                        .DistanceType(GeoDistanceType.Arc)
                        .Unit(DistanceUnit.Kilometers)
                        .Order(SortOrder.Ascending)
                        .Points(new GeoLocation(lat.Value, lon.Value)));
                return sort;
            })
        );

        var items = new List<object>();
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

        var total = response.Total;
        var totalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return Ok(new { items = items, page, pageSize, totalCount = total, totalPages });
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radius = 10,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q.GeoDistance(g => g
                .Field("location")
                .Distance(radius + "km")
                .Location(lat, lon)))
            .Sort(sort =>
            {
                sort.GeoDistance(g => g
                    .Field("location")
                    .DistanceType(GeoDistanceType.Arc)
                    .Unit(DistanceUnit.Kilometers)
                    .Order(SortOrder.Ascending)
                    .Points(new GeoLocation(lat, lon)));
                return sort;
            })
        );

        var items = new List<object>();
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

        return Ok(new { items, page, pageSize, totalCount = response.Total, totalPages = 0 });
    }

    [HttpGet("suggest")]
    public async Task<ActionResult<List<string>>> Suggest([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<string>());

        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .Size(5)
            .Query(qq => qq.MultiMatch(mm => mm
                .Fields(new[] { "name^3" })
                .Query(q)))
        );

        var names = new List<string>();
        if (response.Hits != null)
        {
            foreach (var hit in response.Hits)
            {
                if (hit.Source is JsonElement el && el.TryGetProperty("name", out var nameProp))
                    names.Add(nameProp.GetString() ?? "");
            }
        }

        return Ok(names);
    }

    private static object MapToResult(WorkspaceDocument doc) => new
    {
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
        lat = doc.Location?.Latitude,
        lon = doc.Location?.Longitude,
        doc.Rating,
        doc.ReviewCount
    };
}
