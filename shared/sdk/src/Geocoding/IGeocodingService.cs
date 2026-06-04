namespace DeskMatch.SDK.Geocoding;

public interface IGeocodingService
{
    Task<GeoResult[]> GeocodeAsync(string query, CancellationToken ct = default);
    Task<GeoResult?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default);
}
