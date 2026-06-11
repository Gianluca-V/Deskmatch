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
                : qq.MultiMatch(mm => mm.Fields(new[] { "name^3", "description^2", "address" }).Query(q)))
        );

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

        var total = response.Total;
        var totalPages = total > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return Ok(new { items, page, pageSize, totalCount = total, totalPages });
    }

    private static Dictionary<string, object?> ParseSource(JsonElement el)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in el.EnumerateObject())
        {
            dict[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.Number => prop.Value.TryGetInt64(out var i) ? i : prop.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => prop.Value.GetRawText()
            };
        }
        return dict;
    }
}
