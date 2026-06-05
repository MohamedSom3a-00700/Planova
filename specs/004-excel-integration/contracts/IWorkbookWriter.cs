// IWorkbookWriter — Generates Excel workbooks for export
// Implemented by Planova.Excel.Writers

public interface IWorkbookWriter
{
    Task<ExportResult> WriteAsync(ExportRequest request, IReadOnlyList<Dictionary<string, object>> data, CancellationToken ct);
    bool CanWrite(string format);
}
