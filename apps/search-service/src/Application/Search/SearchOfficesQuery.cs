using DeskMatch.Domain.CQRS;

namespace DeskMatch.SearchService.Application.Search;

public sealed record SearchOfficesQuery : IQuery<SearchOfficesResponse>
{
    public string? Text { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinCapacity { get; init; }
    public List<string>? Amenities { get; init; }
    public double? Lat { get; init; }
    public double? Lon { get; init; }
    public double? RadiusKm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
