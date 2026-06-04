// IBoqExportService — Exports BOQ data to Excel (via Phase 2) or CSV
// Implemented by Planova.Boq.Application.Services

public interface IBoqExportService
{
    Task<ExportResult> ExportToExcelAsync(Guid boqId, ExportOptions options, CancellationToken ct);
    Task<ExportResult> ExportToCsvAsync(Guid boqId, ExportOptions options, CancellationToken ct);
}

public record ExportOptions(
    bool IncludeHeaders,
    bool IncludeSubtotals,
    bool IncludeGrandTotal,
    string? SheetName,
    string OutputPath
);

public record ExportResult(
    string OutputPath,
    long FileSize,
    int ItemCount,
    TimeSpan Duration,
    bool Success
);
