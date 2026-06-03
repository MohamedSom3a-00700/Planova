using Planova.Excel.Models;

namespace Planova.Excel.Readers;

/// <summary>Opens and reads Excel workbooks for browsing and import.</summary>
public interface IWorkbookReader
{
    /// <summary>Opens a workbook and returns metadata about its structure.</summary>
    Task<WorkbookInfo> OpenAsync(string filePath, CancellationToken ct);

    /// <summary>Gets information about a specific worksheet.</summary>
    Task<WorksheetInfo> GetWorksheetInfoAsync(string filePath, string sheetName, CancellationToken ct);

    /// <summary>Returns a paginated preview of worksheet data.</summary>
    Task<PreviewData> PreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct);

    /// <summary>Reads all data rows from a worksheet as dictionaries.</summary>
    Task<IReadOnlyList<Dictionary<string, object>>> ReadAllAsync(string filePath, string sheetName, CancellationToken ct);

    /// <summary>Checks whether the reader can handle the given file path.</summary>
    bool CanRead(string filePath);
}
