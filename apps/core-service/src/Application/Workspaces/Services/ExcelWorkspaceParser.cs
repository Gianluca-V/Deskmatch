using ClosedXML.Excel;
using DeskMatch.CoreService.Application.Workspaces.Models;

namespace DeskMatch.CoreService.Application.Workspaces.Services;

public sealed class ExcelWorkspaceParser : IExcelWorkspaceParser
{
    public BulkParseResult Parse(Stream excelStream)
    {
        using var workbook = new XLWorkbook(excelStream);

        var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Datos", StringComparison.OrdinalIgnoreCase));
        if (sheet is null)
        {
            return new BulkParseResult([], []);
        }

        var validRows = new List<ParsedRow>();
        var invalidRows = new List<ParsedRow>();

        var rows = sheet.RangeUsed()?.RowsUsed();

        if (rows is null)
        {
            return new BulkParseResult([], []);
        }

        foreach (var row in rows)
        {
            if (row.RowNumber() == 1) continue;

            var name = row.Cell(1).GetString();
            var description = row.Cell(2).GetString();
            var address = row.Cell(3).GetString();
            var city = row.Cell(4).GetString();
            var country = row.Cell(5).GetString();
            var capacityStr = row.Cell(6).GetString();
            var pricePerHourStr = row.Cell(7).GetString();
            var pricePerDayStr = row.Cell(8).GetString();
            var pricePerMonthStr = row.Cell(9).GetString();
            var amenitiesStr = row.Cell(10).GetString();
            var imagesStr = row.Cell(11).GetString();

            if (string.IsNullOrWhiteSpace(name)) continue;
            if (name.Contains("ejemplo", StringComparison.OrdinalIgnoreCase)) continue;

            var errors = new List<string>();
            var rowNumber = row.RowNumber();

            if (name.Length > 100) errors.Add("Nombre no puede superar los 100 caracteres");
            if (string.IsNullOrWhiteSpace(address)) errors.Add("Dirección es requerida");
            if (string.IsNullOrWhiteSpace(city)) errors.Add("Ciudad es requerida");
            if (string.IsNullOrWhiteSpace(country)) errors.Add("País es requerido");

            var capacity = 0;
            if (string.IsNullOrWhiteSpace(capacityStr))
            {
                errors.Add("Capacidad es requerida");
            }
            else if (!int.TryParse(capacityStr.Trim(), out capacity) || capacity < 1 || capacity > 9999)
            {
                errors.Add("Capacidad debe ser un número entero entre 1 y 9999");
            }

            var pricePerHour = 0m;
            if (string.IsNullOrWhiteSpace(pricePerHourStr))
            {
                errors.Add("Precio por hora es requerido");
            }
            else if (!decimal.TryParse(pricePerHourStr.Trim(), out pricePerHour) || pricePerHour < 0)
            {
                errors.Add("Precio por hora debe ser un número mayor o igual a 0");
            }

            decimal? pricePerDay = null;
            if (!string.IsNullOrWhiteSpace(pricePerDayStr))
            {
                if (decimal.TryParse(pricePerDayStr.Trim(), out var ppd) && ppd >= 0)
                    pricePerDay = ppd;
                else
                    errors.Add("Precio por día debe ser un número mayor o igual a 0");
            }

            decimal? pricePerMonth = null;
            if (!string.IsNullOrWhiteSpace(pricePerMonthStr))
            {
                if (decimal.TryParse(pricePerMonthStr.Trim(), out var ppm) && ppm >= 0)
                    pricePerMonth = ppm;
                else
                    errors.Add("Precio por mes debe ser un número mayor o igual a 0");
            }

            List<string>? amenities = null;
            if (!string.IsNullOrWhiteSpace(amenitiesStr))
            {
                amenities = amenitiesStr
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }

            List<string>? imageFileNames = null;
            if (!string.IsNullOrWhiteSpace(imagesStr))
            {
                imageFileNames = imagesStr
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }

            var parsed = new ParsedRow(
                rowNumber,
                errors.Count == 0,
                errors,
                name, description, address, city, country,
                capacity, pricePerHour, pricePerDay, pricePerMonth,
                amenities, imageFileNames
            );

            if (parsed.IsValid)
                validRows.Add(parsed);
            else
                invalidRows.Add(parsed);
        }

        return new BulkParseResult(validRows, invalidRows);
    }

}
