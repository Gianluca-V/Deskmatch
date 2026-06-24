using DeskMatch.CoreService.Application.Workspaces.Models;
using DeskMatch.SDK.Geocoding;

namespace DeskMatch.CoreService.Application.Workspaces.Services;

public sealed class WorkspacePreviewService : IWorkspacePreviewService
{
    private readonly IExcelWorkspaceParser _parser;
    private readonly IGeocodingService _geocoding;

    public WorkspacePreviewService(IExcelWorkspaceParser parser, IGeocodingService geocoding)
    {
        _parser = parser;
        _geocoding = geocoding;
    }

    public async Task<BulkPreviewResponse> PreviewAsync(Stream excelStream, CancellationToken ct = default)
    {
        var parseResult = _parser.Parse(excelStream);

        var offices = new List<OfficePreview>();
        var tempId = 0;

        foreach (var row in parseResult.ValidRows)
        {
            tempId++;
            var warnings = new List<string>();
            double? lat = null;
            double? lng = null;

            if (!string.IsNullOrWhiteSpace(row.Address) &&
                !string.IsNullOrWhiteSpace(row.City) &&
                !string.IsNullOrWhiteSpace(row.Country))
            {
                try
                {
                    var query = $"{row.Address}, {row.City}, {row.Country}";
                    var geoResults = await _geocoding.GeocodeAsync(query, ct);
                    if (geoResults.Length > 0)
                    {
                        lat = geoResults[0].Latitude;
                        lng = geoResults[0].Longitude;
                    }
                    else
                    {
                        warnings.Add($"No se pudo determinar la ubicación de \"{row.Name}\". Se creará sin coordenadas.");
                    }
                }
                catch
                {
                    warnings.Add($"Error al geolocalizar \"{row.Name}\". Se creará sin coordenadas.");
                }
            }

            offices.Add(new OfficePreview(
                tempId,
                row.Name!,
                row.Description,
                row.Address,
                row.City,
                row.Country,
                lat,
                lng,
                row.Capacity,
                row.PricePerHour,
                row.PricePerDay,
                row.PricePerMonth,
                row.Amenities,
                row.ImageFileNames,
                warnings
            ));
        }

        foreach (var row in parseResult.InvalidRows)
        {
            tempId++;
            offices.Add(new OfficePreview(
                tempId,
                row.Name ?? $"Fila {row.RowNumber}",
                row.Description,
                row.Address,
                row.City,
                row.Country,
                null,
                null,
                row.Capacity,
                row.PricePerHour,
                row.PricePerDay,
                row.PricePerMonth,
                row.Amenities,
                row.ImageFileNames,
                row.Errors
            ));
        }

        return new BulkPreviewResponse(offices, parseResult.ValidRows.Count + parseResult.InvalidRows.Count);
    }
}
