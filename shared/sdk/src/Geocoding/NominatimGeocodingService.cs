using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace DeskMatch.SDK.Geocoding;

public sealed class NominatimGeocodingService : IGeocodingService, IDisposable
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _rateLimit = new(1, 1);
    private DateTime _lastRequest = DateTime.MinValue;
    private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(1);

    public NominatimGeocodingService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public async Task<GeoResult[]> GeocodeAsync(string query, CancellationToken ct = default)
    {
        var cacheKey = $"geo:{query}";
        if (_cache.TryGetValue(cacheKey, out GeoResult[]? cached) && cached is not null)
            return cached;

        var results = await RequestAsync<NominatimResponse[]>(
            $"/search?q={Uri.EscapeDataString(query)}&format=json&limit=5&addressdetails=1",
            ct
        );

        var mapped = results?.Select(Map).ToArray() ?? [];

        _cache.Set(cacheKey, mapped, TimeSpan.FromMinutes(10));
        return mapped;
    }

    public async Task<GeoResult?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        var cacheKey = $"georev:{latitude:F6},{longitude:F6}";
        if (_cache.TryGetValue(cacheKey, out GeoResult? cached) && cached is not null)
            return cached;

        var result = await RequestAsync<NominatimReverseResponse>(
            $"/reverse?lat={latitude:F6}&lon={longitude:F6}&format=json&addressdetails=1",
            ct
        );

        if (result is null)
            return null;

        var mapped = new GeoResult(
            latitude,
            longitude,
            result.DisplayName ?? "",
            result.Address?.City ?? result.Address?.Town ?? result.Address?.Village,
            result.Address?.Country
        );

        _cache.Set(cacheKey, mapped, TimeSpan.FromMinutes(10));
        return mapped;
    }

    private async Task<T?> RequestAsync<T>(string path, CancellationToken ct) where T : class
    {
        await _rateLimit.WaitAsync(ct);
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequest;
            if (elapsed < MinInterval)
                await Task.Delay(MinInterval - elapsed, ct);

            _lastRequest = DateTime.UtcNow;
            return await _http.GetFromJsonAsync<T>(path, ct);
        }
        finally
        {
            _rateLimit.Release();
        }
    }

    private static GeoResult Map(NominatimResponse r) => new(
        double.Parse(r.Lat),
        double.Parse(r.Lon),
        r.DisplayName ?? "",
        r.Address?.City ?? r.Address?.Town ?? r.Address?.Village,
        r.Address?.Country
    );

    public void Dispose() => _rateLimit.Dispose();

    private sealed class NominatimResponse
    {
        public string Lat { get; set; } = "";
        public string Lon { get; set; } = "";
        public string? DisplayName { get; set; }
        public NominatimAddress? Address { get; set; }
    }

    private sealed class NominatimReverseResponse
    {
        public string? DisplayName { get; set; }
        public NominatimAddress? Address { get; set; }
    }

    private sealed class NominatimAddress
    {
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? Country { get; set; }
    }
}
