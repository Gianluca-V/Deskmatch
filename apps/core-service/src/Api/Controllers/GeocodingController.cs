using DeskMatch.SDK.Geocoding;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/geocode")]
public class GeocodingController : ControllerBase
{
    private readonly IGeocodingService _geocoding;

    public GeocodingController(IGeocodingService geocoding)
    {
        _geocoding = geocoding;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Query parameter 'q' is required" });

        var results = await _geocoding.GeocodeAsync(q, ct);
        return Ok(results);
    }

    [HttpGet("reverse")]
    public async Task<IActionResult> Reverse(
        [FromQuery] double lat,
        [FromQuery] double lon,
        CancellationToken ct)
    {
        var result = await _geocoding.ReverseGeocodeAsync(lat, lon, ct);
        if (result is null)
            return NotFound(new { error = "No results for the given coordinates" });

        return Ok(result);
    }
}
