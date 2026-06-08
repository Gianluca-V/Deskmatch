using DeskMatch.Domain.CQRS;
using DeskMatch.SearchService.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.SearchService.Api.Controllers;

[ApiController]
[Route("api/search")]
[Produces("application/json")]
public sealed class SearchController : ControllerBase
{
    private readonly IQueryHandler<SearchOfficesQuery, SearchOfficesResponse> _searchHandler;

    public SearchController(IQueryHandler<SearchOfficesQuery, SearchOfficesResponse> searchHandler)
    {
        _searchHandler = searchHandler;
    }

    [HttpGet("offices")]
    [ProducesResponseType(typeof(SearchOfficesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchOfficesResponse>> SearchOffices(
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
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var amenityList = amenities?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var query = new SearchOfficesQuery
        {
            Text = q,
            City = city,
            Country = country,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            MinCapacity = minCapacity,
            Amenities = amenityList,
            Lat = lat,
            Lon = lon,
            RadiusKm = radius ?? 10,
            Page = page,
            PageSize = pageSize
        };

        var result = await _searchHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("nearby")]
    [ProducesResponseType(typeof(SearchOfficesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchOfficesResponse>> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radius = 10,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchOfficesQuery
        {
            Lat = lat,
            Lon = lon,
            RadiusKm = radius,
            Page = page,
            PageSize = pageSize
        };

        var result = await _searchHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("suggest")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> Suggest(
        [FromQuery] string? q,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<string>());

        return Ok(new List<string>());
    }
}
