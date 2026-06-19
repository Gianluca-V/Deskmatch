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
        ["cafe"] = "cafe coffee cafeteria",
        ["coffe"] = "coffee",
        ["coffee"] = "coffee cafe",
        ["cafeteria"] = "cafeteria coffee cafe",
        ["gim"] = "gym",
        ["gimnasio"] = "gimnasio gym fitness",
        ["gym"] = "gym gimnasio fitness",
        ["fitness"] = "fitness gym gimnasio",
        ["wifi"] = "wifi internet wireless",
        ["internet"] = "internet wifi wireless",
        ["wireless"] = "wireless wifi internet",
        ["estacionamiento"] = "estacionamiento parking cochera garage",
        ["parking"] = "parking estacionamiento cochera",
        ["cochera"] = "cochera estacionamiento parking garage",
        ["garage"] = "garage estacionamiento parking cochera",
        ["pet"] = "pet mascotas mascota petfriendly pet-friendly",
        ["mascota"] = "mascota mascotas pet petfriendly pet-friendly",
        ["reunion"] = "reunion reuniones meeting sala boardroom",
        ["meeting"] = "meeting reuniones sala boardroom conference",
        ["escritorio"] = "escritorio desk workspace puesto",
        ["desk"] = "desk escritorio workspace puesto",
        ["privado"] = "privado private exclusivo vip",
        ["private"] = "private privado exclusivo vip",
        ["silencioso"] = "silencioso silent quiet tranquilo",
        ["silent"] = "silent silencioso quiet tranquilo",
        ["terraza"] = "terraza rooftop terraza outdoor exterior",
        ["rooftop"] = "rooftop terraza outdoor exterior",
        ["cocina"] = "cocina kitchen kitchenette",
        ["kitchen"] = "kitchen cocina kitchenette",
        ["impresora"] = "impresora printer printing impresion",
        ["printer"] = "printer impresora printing impresion",
        ["accesible"] = "accesible accessible access wheelchair",
        ["accessible"] = "accessible accesible access wheelchair",
        ["24"] = "24 24horas 24-horas horarioextendido",
        ["moderno"] = "moderno modern contemporary sleek",
        ["modern"] = "modern moderno contemporary sleek",
        ["oficina"] = "oficina office workspace despacho",
        ["office"] = "office oficina workspace despacho",
        ["espacio"] = "espacio space workspace area room",
        ["space"] = "space espacio workspace area room",
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
                should.Add(new Dictionary<string, object>
                {
                    ["multi_match"] = new Dictionary<string, object>
                    {
                        ["query"] = q!,
                        ["fields"] = new[] { "name^3", "description^2", "address" },
                        ["fuzziness"] = "AUTO:4,6",
                        ["operator"] = "or"
                    }
                });

                // Each word separately for keyword amenities field (both casings)
                foreach (var word in q!.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var lw = word.ToLowerInvariant();
                    var cw = char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
                    should.Add(new { match = new Dictionary<string, object> { ["amenities"] = lw } });
                    if (lw != cw)
                        should.Add(new { match = new Dictionary<string, object> { ["amenities"] = cw } });
                }
            }

            // k-NN boost
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
                {
                    var expanded = ExpandQuery(am);
                    var terms = expanded.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var allTerms = new List<string>(terms.Length * 2);
                    foreach (var t in terms)
                    {
                        var lw = t.ToLowerInvariant();
                        allTerms.Add(lw);
                        var tc = char.ToUpperInvariant(t[0]) + t[1..].ToLowerInvariant();
                        if (tc != lw)
                            allTerms.Add(tc);
                    }
                    filter.Add(new { terms = new Dictionary<string, object> { ["amenities"] = allTerms.Distinct().ToArray() } });
                }
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
            if (should.Count > 0)
            {
                boolQuery["should"] = should;
                boolQuery["minimum_should_match"] = 1;
            }
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
            ["_source"] = new[] { "name" },
            ["query"] = new Dictionary<string, object>
            {
                ["bool"] = new Dictionary<string, object>
                {
                    ["should"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["match_bool_prefix"] = new Dictionary<string, object>
                            {
                                ["name"] = q
                            }
                        },
                        new { prefix = new Dictionary<string, object> { ["amenities"] = q.ToLowerInvariant() } },
                        new { prefix = new Dictionary<string, object> { ["amenities"] = char.ToUpperInvariant(q[0]) + q[1..].ToLowerInvariant() } }
                    },
                    ["minimum_should_match"] = 1
                }
            }
        };

        var stringResponse = await _client.LowLevel.SearchAsync<StringResponse>(
            "offices", PostData.String(JsonSerializer.Serialize(body)));

        var names = new List<string>();
        if (stringResponse.Body != null)
        {
            using var doc = JsonDocument.Parse(stringResponse.Body);
            var hitsObj = doc.RootElement.GetProperty("hits");
            if (hitsObj.TryGetProperty("hits", out var hits))
            {
                foreach (var hit in hits.EnumerateArray())
                {
                    if (hit.TryGetProperty("_source", out var src) &&
                        src.TryGetProperty("name", out var name))
                    {
                        var n = name.GetString();
                        if (!string.IsNullOrWhiteSpace(n) && !names.Contains(n))
                            names.Add(n);
                    }
                }
            }
        }

        return Ok(names);
    }

    [HttpGet("ai")]
    public async Task<IActionResult> AiSearch(
        [FromQuery] string text,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest(new { error = "text query is required" });

        var aiParams = await ExtractSearchParams(text);

        var expandedQ = ExpandQuery(aiParams.q ?? text);
        var embedding = await _ollama.GetEmbeddingAsync(expandedQ);

        var should = new List<object>();
        var filter = new List<object>();

        // --- should: text BM25 ---
        if (expandedQ.Length > 0)
        {
            should.Add(new Dictionary<string, object>
            {
                ["multi_match"] = new Dictionary<string, object>
                {
                    ["query"] = expandedQ,
                    ["fields"] = new[] { "name^3", "description^2", "address" },
                    ["fuzziness"] = "AUTO:4,6",
                    ["operator"] = "or"
                }
            });

            foreach (var word in expandedQ.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var lw = word.ToLowerInvariant();
                var cw = char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
                should.Add(new { match = new Dictionary<string, object> { ["amenities"] = lw } });
                if (lw != cw)
                    should.Add(new { match = new Dictionary<string, object> { ["amenities"] = cw } });
            }
        }

        // --- should: amenities boosters (from AI extraction) ---
        if (aiParams.amenities?.Length > 0)
        {
            foreach (var am in aiParams.amenities)
            {
                var lw = am.ToLowerInvariant();
                var cw = char.ToUpperInvariant(am[0]) + am[1..].ToLowerInvariant();
                should.Add(new { match = new Dictionary<string, object> { ["amenities"] = lw } });
                if (lw != cw)
                    should.Add(new { match = new Dictionary<string, object> { ["amenities"] = cw } });
            }
        }

        // --- should: capacity booster ---
        if (aiParams.minCapacity.HasValue)
        {
            should.Add(new { range = new Dictionary<string, object> { ["capacity"] = new { gte = aiParams.minCapacity.Value } } });
        }

        // --- filter (strict): city, country, price ---
        if (!string.IsNullOrWhiteSpace(aiParams.city))
            filter.Add(new { match = new Dictionary<string, object> { ["city"] = aiParams.city } });
        if (!string.IsNullOrWhiteSpace(aiParams.country))
            filter.Add(new { match = new Dictionary<string, object> { ["country"] = aiParams.country } });

        if (aiParams.minPrice.HasValue || aiParams.maxPrice.HasValue)
        {
            var range = new Dictionary<string, object>();
            if (aiParams.minPrice.HasValue) range["gte"] = (double)aiParams.minPrice.Value;
            if (aiParams.maxPrice.HasValue) range["lte"] = (double)aiParams.maxPrice.Value;
            filter.Add(new { range = new Dictionary<string, object> { ["pricePerHour"] = range } });
        }

        // --- k-NN boost: descomentar cuando OpenSearch index.knn esté activo ---
        /*
        if (embedding != null && embedding.Length == 768)
        {
            should.Add(new Dictionary<string, object>
            {
                ["script_score"] = new Dictionary<string, object>
                {
                    ["query"] = new { match_all = new { } },
                    ["script"] = new Dictionary<string, object>
                    {
                        ["source"] = "cosineSimilarity(params.query_vector, 'nameVector') + 1.0",
                        ["params"] = new Dictionary<string, object> { ["query_vector"] = embedding }
                    }
                }
            });
        }
        */

        var boolQuery = new Dictionary<string, object>();
        if (should.Count > 0) { boolQuery["should"] = should; boolQuery["minimum_should_match"] = 1; }
        if (filter.Count > 0) boolQuery["filter"] = filter;

        var body = new Dictionary<string, object>
        {
            ["from"] = (page - 1) * pageSize,
            ["size"] = pageSize,
            ["query"] = new { @bool = boolQuery }
        };

        var stringResponse = await _client.LowLevel.SearchAsync<StringResponse>(
            "offices", PostData.String(JsonSerializer.Serialize(body)));

        var items = new List<object>();
        long total = 0;
        ParseLowLevelResponse(stringResponse, ref items, ref total);

        _logger.LogInformation("AI search: text='{Text}', params={Params}, total={Total}",
            text, JsonSerializer.Serialize(aiParams), total);

        var totalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return Ok(new { items, page, pageSize, totalCount = total, totalPages, aiInterpretation = aiParams });
    }

    private static void ParseLowLevelResponse(StringResponse stringResponse, ref List<object> items, ref long total)
    {
        if (stringResponse.Body == null) return;

        using var doc = JsonDocument.Parse(stringResponse.Body);
        var hits = doc.RootElement.GetProperty("hits");
        total = hits.GetProperty("total").ValueKind == JsonValueKind.Object
            ? hits.GetProperty("total").GetProperty("value").GetInt64()
            : hits.GetProperty("total").GetInt64();

        foreach (var hit in hits.GetProperty("hits").EnumerateArray())
            if (hit.TryGetProperty("_source", out var src))
                items.Add(ParseSource(src));
    }

    private async Task<AiSearchParams> ExtractSearchParams(string userText)
    {
        var systemPrompt = @"You are a search parameter extractor for a coworking space (workspace) search engine.
Given a user message describing what office they need, extract these fields as JSON:

- ""q"": search keywords about the space type/style (lowercase). If user says ""moderno"" or ""modern"", put that here.
- ""city"": city name (e.g. ""Buenos Aires"", ""New York"", ""San Francisco"")
- ""country"": country name  
- ""minPrice"", ""maxPrice"": numeric price per hour (decimal). Be VERY careful:
  * ""maximo 10 dolares"", ""hasta 10"", ""no mas de 10"", ""max 10"", ""menos de 10"" -> maxPrice: 10
  * ""minimo 5"", ""desde 5"", ""a partir de 5"", ""min 5"", ""mas de 5"" -> minPrice: 5
  * ""entre 5 y 10"", ""de 5 a 10"" -> minPrice: 5, maxPrice: 10
  * ""barato"", ""economico"" -> maxPrice: 20
  * ""gratis"" -> maxPrice: 0
  * ""caro"", ""premium"", ""lujo"" -> minPrice: 50
- ""minCapacity"": minimum number of people the space must hold
- ""amenities"": array of amenity names (ALWAYS extract). Use exact amenity names: wifi, coffee, gym, parking, ac, printer, meeting, cafeteria. If user says ""cafe"" or ""cafetería"" add ""coffee"" or ""cafeteria"". If user says ""gimnasio"" add ""gym"". RETURN AS ARRAY.
- ""lat"", ""lon"", ""radius"": leave null

IMPORTANT: Always extract whatever you can. Leave unknown fields as null.
Return ONLY valid JSON. Do NOT wrap in backticks.

Example input: ""necesito oficina moderna con wifi y cafe para 5 personas maximo 30 dolares""
Example output: {""q"":""oficina moderna"",""city"":null,""country"":null,""minPrice"":null,""maxPrice"":30,""minCapacity"":5,""amenities"":[""wifi"",""coffee""],""lat"":null,""lon"":null,""radius"":null}

Input: """ + userText + @"""
Output:";

        try
        {
            var result = await _ollama.ChatCompletionAsync("You are a JSON API. Return only valid JSON.", systemPrompt);
            if (string.IsNullOrWhiteSpace(result)) return new AiSearchParams { q = userText };

            var cleaned = result.Trim();
            cleaned = cleaned.Replace("```json", "").Replace("```", "").Trim();
            if (!cleaned.StartsWith("{")) cleaned = "{" + cleaned;
            if (!cleaned.EndsWith("}")) cleaned += "}";

            var parsed = JsonSerializer.Deserialize<AiSearchParams>(cleaned);
            return parsed ?? new AiSearchParams { q = userText };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI search param extraction failed for text: {Text}", userText);
            return new AiSearchParams { q = userText };
        }
    }

    private sealed class AiSearchParams
    {
        public string? q { get; set; }
        public string? city { get; set; }
        public string? country { get; set; }
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
        public int? minCapacity { get; set; }
        public string[]? amenities { get; set; }
        public double? lat { get; set; }
        public double? lon { get; set; }
        public double? radius { get; set; }
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
