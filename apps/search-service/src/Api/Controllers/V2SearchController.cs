using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;

namespace DeskMatch.SearchService.Api.Controllers;

[ApiController]
[Route("api/search")]
[Produces("application/json")]
public sealed class SearchController : ControllerBase
{
    private readonly IOpenSearchClient _client;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IOpenSearchClient client, ILogger<SearchController> logger)
    {
        _client = client;
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
        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(qq => string.IsNullOrWhiteSpace(q)
                ? (QueryContainer)qq.MatchAll()
                : qq.MultiMatch(mm => mm
                    .Fields(new[] { "name^3", "description^2", "amenities^2", "address" })
                    .Query(q)
                    .Fuzziness(Fuzziness.Auto)
                    .Operator(Operator.Or))));

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
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<string>());

        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .Size(5)
            .Query(qq => qq.MultiMatch(mm => mm
                .Fields(new[] { "name^3" })
                .Query(q)
                .Fuzziness(Fuzziness.Auto))));

        var names = new List<string>();
        var rawBody = response.ApiCall?.ResponseBodyInBytes;
        if (rawBody != null && rawBody.Length > 0)
        {
            using var doc = JsonDocument.Parse(rawBody);
            foreach (var hit in doc.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray())
                names.Add(hit.GetProperty("_source").GetProperty("name").GetString() ?? "");
        }
        return Ok(names);
    }

    private List<object> ParseResponseItems(ISearchResponse<object> response)
    {
        var items = new List<object>();
        var rawBody = response.ApiCall?.ResponseBodyInBytes;
        if (rawBody != null && rawBody.Length > 0)
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            if (root.TryGetProperty("hits", out var h1) && h1.TryGetProperty("hits", out var h2))
                foreach (var hit in h2.EnumerateArray())
                    if (hit.TryGetProperty("_source", out var src))
                        items.Add(ParseSource(src));
        }
        return items;
    }

    private static object? ParseElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var i) ? i : el.GetDouble(),
            JsonValueKind.True => true, JsonValueKind.False => false, JsonValueKind.Null => null,
            JsonValueKind.Array => el.EnumerateArray().Select(ParseElement).ToList(),
            JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ParseElement(p.Value)),
            _ => el.GetRawText()
        };
    }

    private static Dictionary<string, object?> ParseSource(JsonElement el)
    {
        var d = new Dictionary<string, object?>();
        foreach (var p in el.EnumerateObject()) d[p.Name] = ParseElement(p.Value);
        return d;
    }
}
