using ClosedXML.Excel;
using Planova.Excel.Models;
using Serilog;

namespace Planova.Excel.Readers;

public class WorkbookReader : IWorkbookReader
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WorkbookReader>();

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".xlsx", ".xlsm" };

    public Task<WorkbookInfo> OpenAsync(string filePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ValidateFilePath(filePath);

        var fileInfo = new FileInfo(filePath);
        Log.Information("Opening workbook: {FilePath}, Size: {FileSize}", filePath, fileInfo.Length);
        using var workbook = new XLWorkbook(filePath);
        var worksheets = workbook.Worksheets.Select(ws => new WorksheetInfo
        {
            Name = ws.Name,
            RowCount = ws.RowsUsed().Count(),
            ColumnCount = ws.ColumnsUsed().Count(),
            Columns = ws.FirstRowUsed()?.Cells().Select(c => c.GetString()).ToList() ?? new()
        }).ToList();

        var info = new WorkbookInfo
        {
            FilePath = filePath,
            FileSize = fileInfo.Exists ? fileInfo.Length : 0,
            Worksheets = worksheets,
            IsValid = true,
            Format = fileInfo.Extension.TrimStart('.').ToLowerInvariant()
        };

        return Task.FromResult(info);
    }

    public Task<WorksheetInfo> GetWorksheetInfoAsync(string filePath, string sheetName, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(sheetName);

        var info = new WorksheetInfo
        {
            Name = ws.Name,
            RowCount = ws.RowsUsed().Count(),
            ColumnCount = ws.ColumnsUsed().Count(),
            Columns = ws.FirstRowUsed()?.Cells().Select(c => c.GetString()).ToList() ?? new()
        };

        return Task.FromResult(info);
    }

    public Task<PreviewData> PreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(sheetName);
        var rows = ws.RowsUsed().ToList();

        if (rows.Count == 0)
        {
            return Task.FromResult(new PreviewData
            {
                WorksheetName = sheetName,
                TotalRowCount = 0,
                PageSize = pageSize,
                CurrentPage = page
            });
        }

        var headerCells = rows[0].Cells().ToList();
        var columns = headerCells.Select(c => c.GetString()).ToList();

        var dataRows = rows.Skip(1).ToList();
        var totalCount = dataRows.Count;
        var skip = (page - 1) * pageSize;
        var pageRows = dataRows.Skip(skip).Take(pageSize).ToList();

        var rowData = pageRows.Select(r =>
        {
            var cells = r.Cells().ToList();
            var dict = new Dictionary<string, object?>();
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = i < cells.Count ? cells[i] : null;
                dict[columns[i]] = cell is not null ? GetCellValue(cell) : null;
            }
            return dict;
        }).ToList();

        var preview = new PreviewData
        {
            WorksheetName = sheetName,
            Columns = columns,
            Rows = rowData,
            TotalRowCount = totalCount,
            PageSize = pageSize,
            CurrentPage = page
        };

        return Task.FromResult(preview);
    }

    public Task<IReadOnlyList<Dictionary<string, object>>> ReadAllAsync(string filePath, string sheetName, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(sheetName);
        var rows = ws.RowsUsed().ToList();

        if (rows.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<Dictionary<string, object>>>(new List<Dictionary<string, object>>());
        }

        var headerCells = rows[0].Cells().ToList();
        var columns = headerCells.Select(c => c.GetString()).ToList();

        var dataRows = rows.Skip(1).Select(r =>
        {
            var cells = r.Cells().ToList();
            var dict = new Dictionary<string, object>();
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = i < cells.Count ? cells[i] : null;
                dict[columns[i]] = cell is not null ? GetCellValue(cell) : string.Empty;
            }
            return dict;
        }).ToList();

        return Task.FromResult<IReadOnlyList<Dictionary<string, object>>>(dataRows);
    }

    public bool CanRead(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        return ext is not null && AllowedExtensions.Contains(ext);
    }

    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (ext is null || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not supported. Only .xlsx and .xlsm files are allowed.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file does not exist.", filePath);

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0)
            throw new InvalidOperationException("The specified file is empty.");

        // Strip external links and block macros
        if (ext == ".xlsm")
        {
            Log.Warning("Opening macro-enabled workbook: {FilePath}. Macros will not be executed.", filePath);
        }
    }

    private static object GetCellValue(IXLCell cell)
    {
        if (cell.TryGetValue<string>(out var text)) return text;
        if (cell.TryGetValue<double>(out var num)) return num;
        if (cell.TryGetValue<int>(out var integer)) return integer;
        if (cell.TryGetValue<bool>(out var boolean)) return boolean;
        if (cell.TryGetValue<DateTime>(out var date)) return date;
        return cell.GetString();
    }
}
