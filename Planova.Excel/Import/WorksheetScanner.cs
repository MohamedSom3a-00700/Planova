using ClosedXML.Excel;

namespace Planova.Excel.Import;

public sealed record WorksheetScanResult(string Name, int Index, int RowCount, bool IsBoqSheet, decimal MatchConfidence);

public sealed class WorksheetScanner
{
    private static readonly string[] NonBoqSheetPatterns =
        ["cover", "summary", "index", "instructions", "notes", "calculation", "title", "contents", "toc", "disclaimer"];

    private static readonly string[] BoqKeywordPatterns =
        ["boq", "bill of quantities", "billofquantities", "schedule of quantities", "scheduleofquantities",
         "pricing", "rate", "quantity", "estimate", "cost estimate", "costestimate"];

    public IReadOnlyList<WorksheetScanResult> Scan(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var workbook = new XLWorkbook(stream);

        var results = new List<WorksheetScanResult>();
        var index = 0;

        foreach (var ws in workbook.Worksheets)
        {
            var name = ws.Name?.Trim() ?? string.Empty;
            var rowCount = ws.LastRowUsed()?.RowNumber() ?? 0;

            var isBoq = !IsNonBoqSheet(name);
            var confidence = 0m;

            if (isBoq)
            {
                confidence = CalculateBoqConfidence(ws);
                if (confidence < 0.2m && !name.Contains("boq", StringComparison.OrdinalIgnoreCase))
                    isBoq = false;
            }

            results.Add(new WorksheetScanResult(name, index, rowCount, isBoq, confidence));
            index++;
        }

        return results.AsReadOnly();
    }

    private static bool IsNonBoqSheet(string name)
    {
        return NonBoqSheetPatterns.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private static decimal CalculateBoqConfidence(IXLWorksheet ws)
    {
        var headerRow = ws.Row(1);
        var lastCell = headerRow.LastCellUsed();
        if (lastCell is null) return 0;

        var matches = 0;
        var checkedCols = 0;

        for (int col = 1; col <= lastCell.Address.ColumnNumber; col++)
        {
            var value = headerRow.Cell(col).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value)) continue;

            checkedCols++;
            if (BoqKeywordPatterns.Any(p => value.Contains(p, StringComparison.OrdinalIgnoreCase)))
                matches++;
        }

        return checkedCols > 0 ? (decimal)matches / checkedCols : 0;
    }
}
