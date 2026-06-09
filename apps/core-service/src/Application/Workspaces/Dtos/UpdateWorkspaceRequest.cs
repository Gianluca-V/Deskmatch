namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public sealed record UpdateWorkspaceRequest(
    Guid CompanyId,
    string Name,
    string? Description,
    string? Address,
    string? City,
    string? Country,
    double? Latitude,
    double? Longitude,
    int Capacity,
    decimal PricePerHour,
    decimal? PricePerDay,
    decimal? PricePerMonth,
    List<string>? Amenities,
    List<string>? Images);
