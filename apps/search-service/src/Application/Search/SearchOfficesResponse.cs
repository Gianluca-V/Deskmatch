namespace DeskMatch.SearchService.Application.Search;

public sealed record SearchOfficesResponse(
    IReadOnlyList<OfficeResult> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages);

public sealed record OfficeResult(
    string Id,
    string Name,
    string? Description,
    string? City,
    string? Country,
    string? Address,
    int Capacity,
    double PricePerHour,
    List<string>? Amenities,
    Dictionary<string, object>? DynamicAttributes,
    double? Lat,
    double? Lon,
    double? Rating,
    int ReviewCount);
