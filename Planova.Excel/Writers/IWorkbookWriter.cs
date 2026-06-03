using Planova.Excel.Models;

namespace Planova.Excel.Writers;

/// <summary>Generates Excel workbooks for export.</summary>
public interface IWorkbookWriter
{
    /// <summary>Writes data to an Excel workbook and saves it.</summary>
    Task<ExportResult> WriteAsync(ExportRequest request, IReadOnlyList<Dictionary<string, object>> data, CancellationToken ct);

    /// <summary>Checks whether the writer supports the given output format.</summary>
    bool CanWrite(string format);
}
