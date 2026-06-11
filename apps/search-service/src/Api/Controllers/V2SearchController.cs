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

    private static readonly Dictionary<string, string> QueryExpansions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cafe"] = "cafe cafe coffee cafeteria",
        ["coffe"] = "coffee",
        ["cafeteria"] = "cafeteria cafeteria coffee cafe",
        ["gim"] = "gym",
        ["gimnasio"] = "gimnasio gym fitness",
        ["wifi"] = "wifi internet wireless",
        ["estacionamiento"] = "estacionamiento parking cochera garage",
        ["parking"] = "parking estacionamiento cochera",
        ["pet"] = "pet mascotas mascota petfriendly pet-friendly",
        ["reunion"] = "reunion reuniones meeting sala boardroom",
        ["meeting"] = "meeting reuniones sala boardroom conference",
        ["escritorio"] = "escritorio desk workspace puesto",
        ["privado"] = "privado private exclusivo vip",
        ["silencioso"] = "silencioso silent quiet tranquilo",
        ["terraza"] = "terraza rooftop terraza outdoor exterior",
        ["cocina"] = "cocina kitchen cocina kitchenette",
        ["impresora"] = "impresora printer printing impresion",
        ["accesible"] = "accesible accessible access wheelchair",
        ["24"] = "24 24horas 24-horas horarioextendido",
        ["moderno"] = "moderno modern contemporary sleek",
        ["oficina"] = "oficina office workspace despacho",
        ["espacio"] = "espacio space workspace area room",
    };

    private static string ExpandQuery(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        var words = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var expanded = new List<string>(words);

        foreach (var word in words)
        {
            if (QueryExpansions.TryGetValue(word, out var expansion))
            {
                foreach (var term in expansion.Split(' '))
                {
                    if (!expanded.Contains(term, StringComparer.OrdinalIgnoreCase))
                        expanded.Add(term);
                }
            }
        }

        return string.Join(" ", expanded).Trim();
    }

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
        var expandedQ = ExpandQuery(q);
        var embedding = expandedQ.Length > 0 ? await _ollama.GetEmbeddingAsync(expandedQ) : null;
        var hasQuery = expandedQ.Length > 0;
        var hasFilters = !string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(country)
            || minPrice.HasValue || maxPrice.HasValue || minCapacity.HasValue
            || !string.IsNullOrWhiteSpace(amenities) || (lat.HasValue && lon.HasValue);

        var body = BuildSearchBody(expandedQ, embedding, city, country, minPrice, maxPrice,
            minCapacity, amenities, lat, lon, radius, page, pageSize, hasQuery, hasFilters);

        var stringResponse = await _client.LowLevel.SearchAsync<StringResponse>(
            "offices", PostData.String(JsonSerializer.Serialize(body)));

        var items = new List<object>();
        long total = 0;

        if (stringResponse.Body != null)
        {
            using var doc = JsonDocument.Parse(stringResponse.Body);
            var hits = doc.RootElement.GetProperty("hits");
            total = hits.GetProperty("total").ValueKind == JsonValueKind.Object
                ? hits.GetProperty("total").GetProperty("value").GetInt64()
                : hits.GetProperty("total").GetInt64();

            foreach (var hit in hits.GetProperty("hits").EnumerateArray())
            {
                if (hit.TryGetProperty("_source", out var src))
                    items.Add(ParseSource(src));
            }
        }

        var totalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return Ok(new { items, page, pageSize, totalCount = total, totalPages });
    }

    private static object BuildSearchBody(string? q, float[]? embedding,
        string? city, string? country, decimal? minPrice, decimal? maxPrice,
        int? minCapacity, string? amenities, double? lat, double? lon,
        double? radius, int page, int pageSize, bool hasQuery, bool hasFilters)
    {
        var body = new Dictionary<string, object>
        {
            ["from"] = (page - 1) * pageSize,
            ["size"] = pageSize,
            ["sort"] = new[] { new Dictionary<string, object> { ["_score"] = new { order = "desc" } } }
        };

        if (!hasQuery && !hasFilters)
        {
            body["query"] = new { match_all = new { } };
        }
        else
        {
            var must = new List<object>();
            var filter = new List<object>();
            var should = new List<object>();

            if (hasQuery)
            {
                must.Add(new Dictionary<string, object>
                {
                    ["multi_match"] = new Dictionary<string, object>
                    {
                        ["query"] = q!,
                        ["fields"] = new[] { "name^3", "description^2", "address" },
                        ["fuzziness"] = "AUTO:4,6",
                        ["operator"] = "or"
                    }
                });

                // Each word separately for keyword amenities field
                foreach (var word in q!.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    should.Add(new { match = new Dictionary<string, object> { ["amenities"] = word } });
                }
            }

            // k-NN boost: apenas se estabilice, descomentar
                /*
                if (embedding != null && embedding.Length == 768)
                {
                    should.Add(new Dictionary<string, object>
                    {
                        ["knn"] = new Dictionary<string, object>
                        {
                            ["nameVector"] = new Dictionary<string, object>
                            {
                                ["vector"] = embedding,
                                ["k"] = 10
                            }
                        }
                    });
                }
                 */

            if (!string.IsNullOrWhiteSpace(city))
                filter.Add(new { match = new Dictionary<string, object> { ["city"] = city } });
            if (!string.IsNullOrWhiteSpace(country))
                filter.Add(new { match = new Dictionary<string, object> { ["country"] = country } });

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                var range = new Dictionary<string, object>();
                if (minPrice.HasValue) range["gte"] = (double)minPrice.Value;
                if (maxPrice.HasValue) range["lte"] = (double)maxPrice.Value;
                filter.Add(new { range = new Dictionary<string, object> { ["pricePerHour"] = range } });
            }

            if (minCapacity.HasValue)
                filter.Add(new { range = new Dictionary<string, object> { ["capacity"] = new { gte = minCapacity.Value } } });

            var amList = amenities?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (amList?.Length > 0)
            {
                foreach (var am in amList)
                    filter.Add(new { match = new Dictionary<string, object> { ["amenities"] = am } });
            }

            if (lat.HasValue && lon.HasValue)
            {
                filter.Add(new
                {
                    geo_distance = new
                    {
                        distance = (radius ?? 10) + "km",
                        location = new { lat = lat.Value, lon = lon.Value }
                    }
                });
            }

            var boolQuery = new Dictionary<string, object>();
            if (must.Count > 0) boolQuery["must"] = must;
            if (should.Count > 0) boolQuery["should"] = should;
            if (filter.Count > 0) boolQuery["filter"] = filter;

            body["query"] = new { @bool = boolQuery };
        }

        return body;
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radius = 10,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var body = new Dictionary<string, object>
        {
            ["from"] = (page - 1) * pageSize,
            ["size"] = pageSize,
            ["query"] = new
            {
                geo_distance = new
                {
                    distance = radius + "km",
                    location = new { lat, lon }
                }
            },
            ["sort"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["_geo_distance"] = new Dictionary<string, object>
                    {
                        ["location"] = new { lat, lon },
                        ["order"] = "asc",
                        ["unit"] = "km"
                    }
                }
            }
        };

        var stringResponse = await _client.LowLevel.SearchAsync<StringResponse>(
            "offices", PostData.String(JsonSerializer.Serialize(body)));

        var items = new List<object>();
        long total = 0;
        if (stringResponse.Body != null)
        {
            using var doc = JsonDocument.Parse(stringResponse.Body);
            var hits = doc.RootElement.GetProperty("hits");
            total = hits.GetProperty("total").ValueKind == JsonValueKind.Object
                ? hits.GetProperty("total").GetProperty("value").GetInt64()
                : hits.GetProperty("total").GetInt64();
            foreach (var hit in hits.GetProperty("hits").EnumerateArray())
                if (hit.TryGetProperty("_source", out var src))
                    items.Add(ParseSource(src));
        }

        return Ok(new { items, page, pageSize, totalCount = total, totalPages = 0 });
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<string>());

        var body = new Dictionary<string, object>
        {
            ["size"] = 5,
            ["query"] = new
            {
                multi_match = new Dictionary<string, object>
                {
                    ["query"] = q,
                    ["fields"] = new[] { "name^3" },
                    ["fuzziness"] = "AUTO:4,6"
                }
            }
        };

        var stringResponse = await _client.LowLevel.SearchAsync<StringResponse>(
            "offices", PostData.String(JsonSerializer.Serialize(body)));

        var names = new List<string>();
        if (stringResponse.Body != null)
        {
            using var doc = JsonDocument.Parse(stringResponse.Body);
            foreach (var hit in doc.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray())
                names.Add(hit.GetProperty("_source").GetProperty("name").GetString() ?? "");
        }
        return Ok(names);
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
        foreach (var p in el.EnumerateObject())
        {
            if (p.Name is "nameVector" or "descriptionVector") continue;
            d[p.Name] = ParseElement(p.Value);
        }
        return d;
    }
}
