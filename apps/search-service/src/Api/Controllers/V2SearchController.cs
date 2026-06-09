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

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public V2SearchController(IOpenSearchClient client) => _client = client;

    [HttpGet("offices")]
    public async Task<IActionResult> SearchOffices([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _client.SearchAsync<object>(s => s
            .Index("offices")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(qq => !string.IsNullOrWhiteSpace(q)
                ? qq.MultiMatch(mm => mm.Fields(new[] { "name^3", "description^2", "address" }).Query(q))
                : (QueryContainer)qq.MatchAll()));

        var items = new List<object>();
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

        return Ok(new { items, page, pageSize, totalCount = response.Total, totalPages = 0 });
    }
}
