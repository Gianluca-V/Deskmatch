using ClosedXML.Excel;

namespace DeskMatch.CoreService.Application.Workspaces.Services;

public sealed class ExcelTemplateService : IExcelTemplateService
{
    public byte[] GenerateTemplate()
    {
        using var workbook = new XLWorkbook();

        var dataSheet = workbook.Worksheets.Add("Datos");
        var headers = new[]
        {
            "Nombre", "Descripción", "Dirección", "Ciudad", "País",
            "Capacidad", "PrecioPorHora", "PrecioPorDía", "PrecioPorMes",
            "Amenities", "Imagenes"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            dataSheet.Cell(1, i + 1).Value = headers[i];
            dataSheet.Cell(1, i + 1).Style.Font.Bold = true;
            dataSheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        string[] exampleData = [
            "Oficina Central", "Espacio amplio con vista panorámica, ideal para equipos de trabajo.",
            "Av. Corrientes 1234", "Buenos Aires", "Argentina",
            "10", "25.00", "180.00", "3500.00",
            "WiFi, Coffee, Parking", "central1.jpg, central2.jpg"
        ];

        for (int i = 0; i < exampleData.Length; i++)
        {
            var cell = dataSheet.Cell(2, i + 1);
            cell.Value = exampleData[i];
            cell.Style.Fill.BackgroundColor = XLColor.Yellow;
        }

        var comment = dataSheet.Cell(2, 1).CreateComment();
        comment.AddText("⬅️ EJEMPLO — Reemplazá o eliminá esta fila antes de subir el archivo");

        dataSheet.Columns().AdjustToContents();

        var instructionsSheet = workbook.Worksheets.Add("Instrucciones");
        instructionsSheet.Cell(1, 1).Value = "Columna";
        instructionsSheet.Cell(1, 2).Value = "Requerido";
        instructionsSheet.Cell(1, 3).Value = "Descripción";
        instructionsSheet.Cell(1, 4).Value = "Ejemplo";

        for (int i = 1; i <= 4; i++)
        {
            instructionsSheet.Cell(1, i).Style.Font.Bold = true;
            instructionsSheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        var instructions = new[]
        {
            new[] { "Nombre", "Sí", "Nombre único del espacio", "\"Oficina Centro\"" },
            new[] { "Descripción", "No", "Descripción del espacio", "\"Amplio piso con...\"" },
            new[] { "Dirección", "Sí", "Dirección (se usa para calcular coordenadas)", "\"Av. Corrientes 1234\"" },
            new[] { "Ciudad", "Sí", "Ciudad", "\"Buenos Aires\"" },
            new[] { "País", "Sí", "País", "\"Argentina\"" },
            new[] { "Capacidad", "Sí", "Número de personas", "10" },
            new[] { "PrecioPorHora", "Sí", "Precio por hora en USD", "25.00" },
            new[] { "PrecioPorDía", "No", "Precio por día", "180.00" },
            new[] { "PrecioPorMes", "No", "Precio por mes", "3500.00" },
            new[] { "Amenities", "No", "Separados por coma", "\"WiFi, Coffee, Parking\"" },
            new[] { "Imagenes", "No", "Nombres de archivo separados por coma (sin rutas). Luego arrastrás las imágenes en el paso de carga.", "\"central1.jpg, central2.jpg\"" },
        };

        for (int row = 0; row < instructions.Length; row++)
        {
            for (int col = 0; col < instructions[row].Length; col++)
            {
                instructionsSheet.Cell(row + 2, col + 1).Value = instructions[row][col];
            }
        }

        instructionsSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
