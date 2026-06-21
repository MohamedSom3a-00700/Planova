namespace Planova.Boq.Application.Models;

public record BoqImportSummary(
    Guid SessionId,
    int SheetsImported,
    int SectionsCreated,
    int ItemsImported,
    decimal TotalAmount);

public record BoqScanPreview(
    int WorksheetsDetected,
    int WorksheetsToImport,
    int EstimatedRows,
    IReadOnlyList<WorksheetPreview> Previews);

public record WorksheetPreview(int WorksheetIndex, string Name, int RowCount, int MatchedColumns);
