using ClosedXML.Excel;
using Planova.Excel.Models;

namespace Planova.Excel.Writers;

public class WorkbookWriter : IWorkbookWriter
{
    public Task<ExportResult> WriteAsync(ExportRequest request, IReadOnlyList<Dictionary<string, object>> data, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(request.SheetName);

        if (request.IncludeHeaders && request.SelectedColumns.Count > 0)
        {
            for (int i = 0; i < request.SelectedColumns.Count; i++)
            {
                ws.Cell(1, i + 1).Value = request.SelectedColumns[i];
            }
        }

        var dataStartRow = request.IncludeHeaders ? 2 : 1;

        for (int row = 0; row < data.Count; row++)
        {
            ct.ThrowIfCancellationRequested();
            var record = data[row];
            for (int col = 0; col < request.SelectedColumns.Count; col++)
            {
                var fieldName = request.SelectedColumns[col];
                if (record.TryGetValue(fieldName, out var value) && value is not null)
                {
                    ws.Cell(row + dataStartRow, col + 1).Value = ConvertToXLCellValue(value);
                }
            }
        }

        ws.Columns().AdjustToContents();

        var outputDir = Path.GetDirectoryName(request.OutputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        workbook.SaveAs(request.OutputPath);

        var fileInfo = new FileInfo(request.OutputPath);
        var result = new ExportResult
        {
            TotalRecords = data.Count,
            OutputPath = request.OutputPath,
            FileSize = fileInfo.Exists ? fileInfo.Length : 0,
            Success = fileInfo.Exists
        };

        return Task.FromResult(result);
    }

    public bool CanWrite(string format)
    {
        return format is ".xlsx" or "xlsx";
    }

    private static XLCellValue ConvertToXLCellValue(object value)
    {
        return value switch
        {
            string s => s,
            int i => i,
            long l => l,
            double d => d,
            decimal m => (double)m,
            bool b => b,
            DateTime dt => dt,
            _ => value.ToString() ?? string.Empty
        };
    }
}
