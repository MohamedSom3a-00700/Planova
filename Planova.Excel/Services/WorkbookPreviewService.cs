using Planova.Excel.Models;
using Planova.Excel.Readers;

namespace Planova.Excel.Services;

public class WorkbookPreviewService : IWorkbookPreviewService
{
    private readonly IWorkbookReader _reader;

    public WorkbookPreviewService(IWorkbookReader reader)
    {
        _reader = reader;
    }

    public async Task<WorkbookInfo> GetWorkbookInfoAsync(string filePath, CancellationToken ct)
    {
        return await _reader.OpenAsync(filePath, ct);
    }

    public async Task<PreviewData> GetPreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct)
    {
        return await _reader.PreviewAsync(filePath, sheetName, page, pageSize, ct);
    }

    public async IAsyncEnumerable<string> SearchAsync(string filePath, string query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var workbook = await _reader.OpenAsync(filePath, ct);
        foreach (var ws in workbook.Worksheets)
        {
            ct.ThrowIfCancellationRequested();
            if (ws.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                yield return ws.Name;
            }
        }
    }
}
