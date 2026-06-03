using Planova.Excel.Models;

namespace Planova.Excel.Services;

/// <summary>Provides read-only preview of worksheet data for the Workbook Browser.</summary>
public interface IWorkbookPreviewService
{
    /// <summary>Gets workbook metadata including worksheet listing.</summary>
    Task<WorkbookInfo> GetWorkbookInfoAsync(string filePath, CancellationToken ct);

    /// <summary>Gets a paginated preview of a worksheet's data.</summary>
    Task<PreviewData> GetPreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct);

    /// <summary>Searches worksheet names for the given query.</summary>
    IAsyncEnumerable<string> SearchAsync(string filePath, string query, CancellationToken ct);
}
