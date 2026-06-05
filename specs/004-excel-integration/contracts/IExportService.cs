// IExportService — Orchestrates the full export workflow: query, format, write workbook
// Implemented by Planova.Excel.Services

public interface IExportService
{
    Task<ExportRequest> BuildRequestAsync(string entityType, CancellationToken ct);
    Task<ExportResult> ExportAsync(ExportRequest request, IProgress<int> progress, CancellationToken ct);
    IReadOnlyList<string> GetExportableEntityTypes();
}
