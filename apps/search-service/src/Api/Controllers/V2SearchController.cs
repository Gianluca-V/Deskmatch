using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;

namespace DeskMatch.SearchService.Api.Controllers;

[ApiController]
[Route("api/v2/search")]
[Produces("application/json")]
public sealed class V2SearchController : ControllerBase
{
    private readonly IOpenSearchClient _client;
    private readonly ILogger<V2SearchController> _logger;

    public V2SearchController(IOpenSearchClient client, ILogger<V2SearchController> logger)
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
            .Query(qq => qq.Bool(b =>
            {
                if (!string.IsNullOrWhiteSpace(q))
                    b.Must(mu => mu.MultiMatch(mm => mm
                        .Fields(new[] { "name^3", "description^2", "address" })
                        .Query(q)));

                if (!string.IsNullOrWhiteSpace(city)) b.Filter(f => f.Term("city", city));
                if (!string.IsNullOrWhiteSpace(country)) b.Filter(f => f.Term("country", country));

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

                if (lat.HasValue && lon.HasValue)
                    b.Filter(f => f.GeoDistance(g => g
                        .Field("location")
                        .Distance((radius ?? 10) + "km")
                        .Location(lat.Value, lon.Value)));

                return b;
            }))
            .Sort(sort =>
            {
                sort.Field("_score", SortOrder.Descending);
                sort.Field("rating", SortOrder.Descending);
                return sort;
            })
        );

        var items = ParseHitsResponse(response);
        var totalPages = response.Total > 0 ? (int)Math.Ceiling(response.Total / (double)pageSize) : 0;
        return Ok(new { items, page, pageSize, totalCount = response.Total, totalPages });
    }

    private List<Dictionary<string, object?>> ParseHitsResponse(ISearchResponse<object> response)
    {
        var items = new List<Dictionary<string, object?>>();

        try
        {
            var rawBody = response.ApiCall?.ResponseBodyInBytes;
            if (rawBody != null && rawBody.Length > 0)
            {
                using var doc = JsonDocument.Parse(rawBody);
                var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");
                foreach (var hit in hits.EnumerateArray())
                {
                    var source = hit.GetProperty("_source");
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in source.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString(),
                            JsonValueKind.Number => prop.Value.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => null,
                            _ => prop.Value.ToString()
                        };
                    }
                    items.Add(dict);
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
                {
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in el.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString(),
                            JsonValueKind.Number => prop.Value.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            _ => prop.Value.ToString()
                        };
                    }
                    items.Add(dict);
                }
            }
        }

        return items;
    }
}
