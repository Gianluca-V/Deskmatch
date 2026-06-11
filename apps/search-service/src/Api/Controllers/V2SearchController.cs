using System.Text;
using System.Text.Json;
using DeskMatch.SDK.Ollama;
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Client;
using OpenSearch.Net;

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
        var embedding = await _ollama.GetEmbeddingAsync(q ?? "");
        var hasFilters = !string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(country)
            || minPrice.HasValue || maxPrice.HasValue || minCapacity.HasValue
            || (lat.HasValue && lon.HasValue)
            || !string.IsNullOrWhiteSpace(amenities);

        ISearchResponse<object> response;

        if (string.IsNullOrWhiteSpace(q) && !hasFilters)
        {
            response = await _client.SearchAsync<object>(s => s
                .Index("offices")
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Query(qq => qq.MatchAll()));
        }
        else if (embedding != null)
        {
            response = await _client.SearchAsync<object>(s =>
                BuildHybridQuery(s, q, city, country, minPrice, maxPrice, minCapacity, amenities, lat, lon, radius, page, pageSize, embedding));
        }
        else
        {
            response = await _client.SearchAsync<object>(s =>
                BuildBm25Query(s, q, city, country, minPrice, maxPrice, minCapacity, amenities, lat, lon, radius, page, pageSize));
        }

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

    private static SearchDescriptor<object> BuildBm25Query(
        SearchDescriptor<object> s, string? q,
        string? city, string? country, decimal? minPrice, decimal? maxPrice,
        int? minCapacity, string? amenities, double? lat, double? lon,
        double? radius, int page, int pageSize)
    {
        return s.Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(qq => qq.Bool(b =>
            {
                if (!string.IsNullOrWhiteSpace(q))
                {
                    b.Must(mu => mu.MultiMatch(mm => mm
                        .Fields(new[] { "name^3", "description^2", "amenities^2", "address" })
                        .Query(q)
                        .Fuzziness(Fuzziness.Auto)
                        .Operator(Operator.Or)));
                }

                b.Should(sh => sh
                    .Match(m => m.Field("amenities").Query(q ?? "").Boost(2)));

                AddFilters(b, city, country, minPrice, maxPrice, minCapacity, amenities, lat, lon, radius);

                return b;
            }))
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
            });
    }

    private static SearchDescriptor<object> BuildHybridQuery(
        SearchDescriptor<object> s, string? q,
        string? city, string? country, decimal? minPrice, decimal? maxPrice,
        int? minCapacity, string? amenities, double? lat, double? lon,
        double? radius, int page, int pageSize, float[] embedding)
    {
        var queryText = q ?? "";

        return s.Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(qq => qq.Bool(b =>
            {
                if (!string.IsNullOrWhiteSpace(q))
                {
                    b.Must(mu => mu.MultiMatch(mm => mm
                        .Fields(new[] { "name^3", "description^2", "amenities^2", "address" })
                        .Query(queryText)
                        .Fuzziness(Fuzziness.Auto)
                        .Operator(Operator.Or)));
                }

                b.Should(sh => sh
                    .Match(m => m.Field("amenities").Query(queryText).Boost(2)));

                b.Should(sh => sh
                    .Script(scr => scr
                        .Script(sc => sc
                            .Source("cosineSimilarity(params.query_vector, 'nameVector') + 1.0")
                            .Params(d => d.Add("query_vector", embedding)))));

                b.Should(sh => sh
                    .Script(scr => scr
                        .Script(sc => sc
                            .Source("cosineSimilarity(params.query_vector, 'descriptionVector') + 0.5")
                            .Params(d => d.Add("query_vector", embedding)))));

                AddFilters(b, city, country, minPrice, maxPrice, minCapacity, amenities, lat, lon, radius);

                return b;
            }))
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
            });
    }

    private static void AddFilters(
        BoolQueryDescriptor<object> b,
        string? city, string? country, decimal? minPrice, decimal? maxPrice,
        int? minCapacity, string? amenities, double? lat, double? lon, double? radius)
    {
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

        if (lat.HasValue && lon.HasValue)
            b.Filter(f => f.GeoDistance(g => g
                .Field("location")
                .Distance((radius ?? 10) + "km")
                .Location(lat.Value, lon.Value)));

        var amList = amenities?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (amList?.Length > 0)
            b.Filter(f => f.Terms(t => t.Field("amenities").Terms(amList)));
    }

    private List<object> ParseResponseItems(ISearchResponse<object> response)
    {
        var items = new List<object>();
        var rawBody = response.ApiCall?.ResponseBodyInBytes;
        if (rawBody != null && rawBody.Length > 0)
        {
            using var doc = JsonDocument.Parse(rawBody);
            foreach (var hit in doc.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray())
            {
                items.Add(ParseSource(hit.GetProperty("_source")));
            }
        }
        else if (response.Hits != null)
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
