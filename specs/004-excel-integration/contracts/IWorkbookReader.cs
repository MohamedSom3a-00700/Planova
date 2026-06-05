// IWorkbookReader — Opens and reads Excel workbooks for browsing and import
// Implemented by Planova.Excel.Readers

public interface IWorkbookReader
{
    Task<WorkbookInfo> OpenAsync(string filePath, CancellationToken ct);
    Task<WorksheetInfo> GetWorksheetInfoAsync(string filePath, string sheetName, CancellationToken ct);
    Task<PreviewData> PreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<Dictionary<string, object>>> ReadAllAsync(string filePath, string sheetName, CancellationToken ct);
    bool CanRead(string filePath);
}
