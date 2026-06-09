using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record UpdateWorkspaceCommand(
    Guid Id,
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
    List<string>? Images,
    List<WorkspaceAttributeInput>? DynamicAttributes) : ICommand;
