// IWorkbookPreviewService — Provides read-only preview of worksheet data for the Workbook Browser
// Implemented by Planova.Excel.Services

public interface IWorkbookPreviewService
{
    Task<WorkbookInfo> GetWorkbookInfoAsync(string filePath, CancellationToken ct);
    Task<PreviewData> GetPreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct);
    IAsyncEnumerable<string> SearchAsync(string filePath, string query, CancellationToken ct);
}
