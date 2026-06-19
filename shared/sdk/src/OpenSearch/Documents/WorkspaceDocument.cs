using OpenSearch.Client;

namespace DeskMatch.SDK.OpenSearch.Documents;

public sealed class WorkspaceDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public int Capacity { get; set; }
    public double PricePerHour { get; set; }
    public List<string>? Amenities { get; set; }
    public GeoLocation? Location { get; set; }
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string>? Images { get; set; }
    public Dictionary<string, object>? DynamicAttributes { get; set; }
    public float[]? NameVector { get; set; }
    public float[]? DescriptionVector { get; set; }
}
