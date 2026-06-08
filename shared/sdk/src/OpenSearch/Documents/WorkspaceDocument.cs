using OpenSearch.Client;

namespace DeskMatch.SDK.OpenSearch.Documents;

public sealed record WorkspaceDocument
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? Address { get; init; }
    public int Capacity { get; init; }
    public double PricePerHour { get; init; }
    public List<string>? Amenities { get; init; }
    public GeoLocation? Location { get; init; }
    public double? Rating { get; init; }
    public int ReviewCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Dictionary<string, object>? DynamicAttributes { get; init; }
    public float[]? NameVector { get; init; }
    public float[]? DescriptionVector { get; init; }
}
