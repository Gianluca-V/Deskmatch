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
        var must = new List<QueryContainer>();
        var filter = new List<QueryContainer>();

        if (!string.IsNullOrWhiteSpace(q))
        {
            must.Add(new QueryContainer(new MultiMatchQuery
            {
                Fields = new[] { "name^3", "description^2", "amenities^2", "address" },
                Query = q,
                Fuzziness = Fuzziness.Auto,
                Operator = Operator.Or
            }));
        }

        if (!string.IsNullOrWhiteSpace(city))
            filter.Add(new QueryContainer(new MatchQuery { Field = "city", Query = city }));
        if (!string.IsNullOrWhiteSpace(country))
            filter.Add(new QueryContainer(new MatchQuery { Field = "country", Query = country }));

        if (minPrice.HasValue || maxPrice.HasValue)
        {
            var range = new NumericRangeQuery { Field = "pricePerHour" };
            if (minPrice.HasValue) range.GreaterThanOrEqualTo = (double)minPrice.Value;
            if (maxPrice.HasValue) range.LessThanOrEqualTo = (double)maxPrice.Value;
            filter.Add(new QueryContainer(range));
        }

        if (minCapacity.HasValue)
            filter.Add(new QueryContainer(new NumericRangeQuery
            {
                Field = "capacity",
                GreaterThanOrEqualTo = minCapacity.Value
            }));

        var amList = amenities?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (amList?.Length > 0)
        {
            foreach (var am in amList)
                filter.Add(new QueryContainer(new MatchQuery { Field = "amenities", Query = am }));
        }

        if (lat.HasValue && lon.HasValue)
            filter.Add(new QueryContainer(new GeoDistanceQuery
            {
                Field = "location",
                Distance = (radius ?? 10) + "km",
                Location = new GeoLocation(lat.Value, lon.Value)
            }));

        QueryContainer query;
        if (must.Count == 0 && filter.Count == 0)
        {
            query = new QueryContainer(new MatchAllQuery());
        }
        else
        {
            var boolQuery = new BoolQuery();
            if (must.Count > 0) boolQuery.Must = must;
            if (filter.Count > 0) boolQuery.Filter = filter;
            query = new QueryContainer(boolQuery);
        }

        var response = await _client.SearchAsync<object>(new SearchRequest("offices")
        {
            From = (page - 1) * pageSize,
            Size = pageSize,
            Query = query
        });

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
        var response = await _client.SearchAsync<object>(new SearchRequest("offices")
        {
            From = (page - 1) * pageSize,
            Size = pageSize,
            Query = new QueryContainer(new GeoDistanceQuery
            {
                Field = "location",
                Distance = radius + "km",
                Location = new GeoLocation(lat, lon)
            })
        });

        var items = ParseResponseItems(response);
        return Ok(new { items, page, pageSize, totalCount = response.Total, totalPages = 0 });
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<string>());

        var response = await _client.SearchAsync<object>(new SearchRequest("offices")
        {
            Size = 5,
            Query = new QueryContainer(new MultiMatchQuery
            {
                Fields = new[] { "name^3" },
                Query = q,
                Fuzziness = Fuzziness.Auto
            })
        });

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

    private static object? ParseElement(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Number => el.TryGetInt64(out var i) ? i : el.GetDouble(),
        JsonValueKind.True => true, JsonValueKind.False => false, JsonValueKind.Null => null,
        JsonValueKind.Array => el.EnumerateArray().Select(ParseElement).ToList(),
        JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ParseElement(p.Value)),
        _ => el.GetRawText()
    };

    private static Dictionary<string, object?> ParseSource(JsonElement el)
    {
        var d = new Dictionary<string, object?>();
        foreach (var p in el.EnumerateObject()) d[p.Name] = ParseElement(p.Value);
        return d;
    }
}
