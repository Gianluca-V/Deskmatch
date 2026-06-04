namespace DeskMatch.SDK.Geocoding;

public record GeoResult(
    double Latitude,
    double Longitude,
    string DisplayName,
    string? City,
    string? Country
);
