namespace DeskMatch.CoreService.Application.Workspaces.Models;

public sealed record ParsedRow(
    int RowNumber,
    bool IsValid,
    List<string> Errors,
    string? Name,
    string? Description,
    string? Address,
    string? City,
    string? Country,
    int Capacity,
    decimal PricePerHour,
    decimal? PricePerDay,
    decimal? PricePerMonth,
    List<string>? Amenities,
    List<string>? ImageFileNames
);

public sealed record BulkParseResult(
    List<ParsedRow> ValidRows,
    List<ParsedRow> InvalidRows
);

public sealed record OfficePreview(
    int TempId,
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
    List<string>? ImageFileNames,
    List<string> Warnings
);

public sealed record BulkPreviewResponse(
    List<OfficePreview> Offices,
    int TotalRows
);

public sealed record OfficeData(
    int TempId,
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
    List<string>? ImageFileNames
);

public sealed record BulkCreateResponse(
    int CreatedCount,
    int TotalRows,
    List<BulkError> Errors
);

public sealed record BulkError(
    int Row,
    string OfficeName,
    string Message
);
