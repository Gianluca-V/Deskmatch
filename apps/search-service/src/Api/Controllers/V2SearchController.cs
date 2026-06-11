using System.Text;
using System.Text.Json;
using DeskMatch.SDK.Ollama;
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
    private readonly ILogger<SearchController> _logger;

    public SearchController(IOpenSearchClient client, IOllamaClient ollama, ILogger<SearchController> logger)
    {
        _client = client;
        _ollama = ollama;
        _logger = logger;
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
        [FromQuery] int pageSize = 10)
    {
        var hasQuery = !string.IsNullOrWhiteSpace(q);
        var hasFilters = !string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(country)
            || minPrice.HasValue || maxPrice.HasValue || minCapacity.HasValue
            || (lat.HasValue && lon.HasValue)
            || !string.IsNullOrWhiteSpace(amenities);

        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(qq =>
            {
                if (!hasQuery && !hasFilters)
                    return (QueryContainer)qq.MatchAll();

                return qq.MultiMatch(mm =>
                    mm.Fields(new[] { "name^3", "description^2", "amenities^2", "address" })
                      .Query(q ?? "")
                      .Fuzziness(Fuzziness.Auto)
                      .Operator(Operator.Or));
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

        var items = ParseResponseItems(response);
        var total = response.Total;
        var totalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return Ok(new { items, page, pageSize, totalCount = total, totalPages });
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

        var items = ParseResponseItems(response);
        return Ok(new { items, page, pageSize, totalCount = response.Total, totalPages = 0 });
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<string>());

        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .Size(5)
            .Query(qq => qq.MultiMatch(mm => mm
                .Fields(new[] { "name^3" })
                .Query(q)
                .Fuzziness(Fuzziness.Auto)))
        );

        var names = new List<string>();
        var rawBody = response.ApiCall?.ResponseBodyInBytes;
        if (rawBody != null && rawBody.Length > 0)
        {
            using var doc = JsonDocument.Parse(rawBody);
            foreach (var hit in doc.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray())
            {
                var name = hit.GetProperty("_source").GetProperty("name").GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name);
            }
        }

        return Ok(names);
    }

    private List<object> ParseResponseItems(ISearchResponse<object> response)
    {
        var items = new List<object>();
        try
        {
            var rawBody = response.ApiCall?.ResponseBodyInBytes;
            if (rawBody != null && rawBody.Length > 0)
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("hits", out var hitsContainer) &&
                    hitsContainer.TryGetProperty("hits", out var hitsArray))
                {
                    foreach (var hit in hitsArray.EnumerateArray())
                    {
                        if (hit.TryGetProperty("_source", out var source))
                            items.Add(ParseSource(source));
                    }
                }
                return items;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse raw OpenSearch response body");
        }

        if (response.Hits != null)
        {
            foreach (var hit in response.Hits)
            {
                if (hit.Source is JsonElement el)
                    items.Add(ParseSource(el));
            }
        }

        return items;
    }

    private static object? ParseElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var i) ? i : el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => el.EnumerateArray().Select(ParseElement).ToList(),
            JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ParseElement(p.Value)),
            _ => el.GetRawText()
        };
    }

    private static Dictionary<string, object?> ParseSource(JsonElement el)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in el.EnumerateObject())
        {
            dict[prop.Name] = ParseElement(prop.Value);
        }
        return dict;
    }
}
