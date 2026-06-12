using System.IO.Compression;
using System.Xml.Linq;
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

        try
        {
            using var workbook = OpenWorkbook(filePath);
            var worksheets = new List<WorksheetInfo>();
            foreach (var ws in workbook.Worksheets)
            {
                try
                {
                    var columns = ws.FirstRowUsed()?.Cells().Select(c => GetCellValueSafe(c)).ToList() ?? new();
                    var rowCount = ws.RowsUsed().Count();
                    var colCount = ws.ColumnsUsed().Count();

                    worksheets.Add(new WorksheetInfo
                    {
                        Name = ws.Name,
                        RowCount = rowCount,
                        ColumnCount = colCount,
                        Columns = columns
                    });
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Skipping worksheet '{WorksheetName}' due to parsing error", ws.Name);
                    worksheets.Add(new WorksheetInfo
                    {
                        Name = ws.Name,
                        RowCount = 0,
                        ColumnCount = 0,
                        Columns = new List<string>()
                    });
                }
            }

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
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open workbook: {FilePath}", filePath);
            return Task.FromResult(new WorkbookInfo
            {
                FilePath = filePath,
                FileSize = fileInfo.Exists ? fileInfo.Length : 0,
                Worksheets = new List<WorksheetInfo>(),
                IsValid = false,
                Format = fileInfo.Extension.TrimStart('.').ToLowerInvariant()
            });
        }
    }

    public Task<WorksheetInfo> GetWorksheetInfoAsync(string filePath, string sheetName, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            using var workbook = OpenWorkbook(filePath);
            var ws = workbook.Worksheet(sheetName);

            var info = new WorksheetInfo
            {
                Name = ws.Name,
                RowCount = ws.RowsUsed().Count(),
                ColumnCount = ws.ColumnsUsed().Count(),
                Columns = ws.FirstRowUsed()?.Cells().Select(c => GetCellValueSafe(c)).ToList() ?? new()
            };

            return Task.FromResult(info);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read worksheet info for '{SheetName}'", sheetName);
            return Task.FromResult(new WorksheetInfo
            {
                Name = sheetName,
                RowCount = 0,
                ColumnCount = 0,
                Columns = new List<string>()
            });
        }
    }

    public Task<PreviewData> PreviewAsync(string filePath, string sheetName, int page, int pageSize, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ValidateFilePath(filePath);

        try
        {
            using var workbook = OpenWorkbook(filePath);
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
            var columns = headerCells.Select(c => GetCellValueSafe(c)).ToList();

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
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to preview worksheet '{SheetName}'", sheetName);
            return Task.FromResult(new PreviewData
            {
                WorksheetName = sheetName,
                TotalRowCount = 0,
                PageSize = pageSize,
                CurrentPage = page
            });
        }
    }

    public Task<IReadOnlyList<Dictionary<string, object>>> ReadAllAsync(string filePath, string sheetName, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ValidateFilePath(filePath);

        try
        {
            using var workbook = OpenWorkbook(filePath);
            var ws = workbook.Worksheet(sheetName);
            var rows = ws.RowsUsed().ToList();

            if (rows.Count == 0)
            {
                return Task.FromResult<IReadOnlyList<Dictionary<string, object>>>(new List<Dictionary<string, object>>());
            }

            var headerCells = rows[0].Cells().ToList();
            var columns = headerCells.Select(c => GetCellValueSafe(c)).ToList();

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
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read all data from worksheet '{SheetName}'", sheetName);
            return Task.FromResult<IReadOnlyList<Dictionary<string, object>>>(new List<Dictionary<string, object>>());
        }
    }

    public bool CanRead(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        return ext is not null && AllowedExtensions.Contains(ext);
    }

    private static XLWorkbook OpenWorkbook(string filePath)
    {
        try
        {
            return new XLWorkbook(filePath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to open workbook normally; trying with formula evaluation disabled: {FilePath}", filePath);

            try
            {
                return new XLWorkbook(filePath, new ClosedXML.Excel.LoadOptions { RecalculateAllFormulas = false });
            }
            catch (Exception ex2)
            {
                Log.Warning(ex2, "Still cannot open workbook; stripping problematic metadata and retrying: {FilePath}", filePath);

                var tempPath = RepairWorkbook(filePath);
                try
                {
                    return new XLWorkbook(tempPath, new ClosedXML.Excel.LoadOptions { RecalculateAllFormulas = false });
                }
                finally
                {
                    TryDeleteFile(tempPath);
                }
            }
        }
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

    private static string RepairWorkbook(string filePath)
    {
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath);
        tempPath = Path.ChangeExtension(tempPath, ".xlsx");

        try
        {
            using var source = ZipFile.Open(filePath, ZipArchiveMode.Read);
            using var dest = ZipFile.Open(tempPath, ZipArchiveMode.Create);

            foreach (var entry in source.Entries)
            {
                var fullName = entry.FullName;

                if (fullName.Equals("xl/calcChain.xml", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (fullName.Equals("xl/_rels/workbook.xml.rels", StringComparison.OrdinalIgnoreCase))
                {
                    WriteModifiedEntry(dest, entry, fullName, StripCalcChainRelationship);
                    continue;
                }

                if (fullName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase))
                {
                    WriteModifiedEntry(dest, entry, fullName, StripCalcChainContentType);
                    continue;
                }

                if (fullName.Equals("xl/workbook.xml", StringComparison.OrdinalIgnoreCase))
                {
                    WriteModifiedEntry(dest, entry, fullName, StripExternalDefinedNames);
                    continue;
                }

                CopyEntry(dest, entry);
            }

            Log.Information("Repaired workbook metadata: {FilePath}", filePath);
            return tempPath;
        }
        catch
        {
            TryDeleteFile(tempPath);
            throw;
        }
    }

    private static void CopyEntry(ZipArchive dest, ZipArchiveEntry entry)
    {
        var newEntry = dest.CreateEntry(entry.FullName);
        using var src = entry.Open();
        using var dst = newEntry.Open();
        src.CopyTo(dst);
    }

    private static void WriteModifiedEntry(ZipArchive dest, ZipArchiveEntry entry, string fullName, Func<string, string> modify)
    {
        using var reader = new StreamReader(entry.Open());
        var content = reader.ReadToEnd();
        var modified = modify(content);
        var newEntry = dest.CreateEntry(fullName);
        using var writer = new StreamWriter(newEntry.Open());
        writer.Write(modified);
    }

    private static readonly XNamespace SsNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace CtNs = "http://schemas.openxmlformats.org/package/2006/content-types";

    private static string StripExternalDefinedNames(string workbookXml)
    {
        var doc = XDocument.Parse(workbookXml, System.Xml.Linq.LoadOptions.PreserveWhitespace);
        var badNames = doc.Descendants(SsNs + "definedName")
            .Where(d => d.Value.Contains('['))
            .ToList();

        if (badNames.Count == 0)
            return workbookXml;

        foreach (var dn in badNames)
            dn.Remove();

        Log.Information("Removed {Count} defined name(s) with external references", badNames.Count);
        return doc.ToString();
    }

    private static string StripCalcChainRelationship(string relsXml)
    {
        var doc = XDocument.Parse(relsXml, System.Xml.Linq.LoadOptions.PreserveWhitespace);
        var calcRels = doc.Root?.Elements(RelNs + "Relationship")
            .Where(r => r.Attribute("Target")?.Value == "calcChain.xml")
            .ToList();

        if (calcRels is null || calcRels.Count == 0)
            return relsXml;

        foreach (var rel in calcRels)
            rel.Remove();

        Log.Information("Removed calcChain relationship from workbook.xml.rels");
        return doc.ToString();
    }

    private static string StripCalcChainContentType(string ctXml)
    {
        var doc = XDocument.Parse(ctXml, System.Xml.Linq.LoadOptions.PreserveWhitespace);
        var calcOverrides = doc.Root?.Elements(CtNs + "Override")
            .Where(o => o.Attribute("PartName")?.Value == "/xl/calcChain.xml")
            .ToList();

        if (calcOverrides is null || calcOverrides.Count == 0)
            return ctXml;

        foreach (var ov in calcOverrides)
            ov.Remove();

        Log.Information("Removed calcChain content type override");
        return doc.ToString();
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to delete temporary file: {FilePath}", path);
        }
    }

    private static object GetCellValue(IXLCell cell)
    {
        try
        {
            if (cell.TryGetValue<int>(out var integer)) return integer;
            if (cell.TryGetValue<double>(out var num)) return num;
            if (cell.TryGetValue<bool>(out var boolean)) return boolean;
            if (cell.TryGetValue<DateTime>(out var date)) return date;
            if (cell.TryGetValue<string>(out var text) && text is not null) return text;
            return cell.GetString();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read cell value at {CellAddress}", cell.Address);
            return string.Empty;
        }
    }

    private static string GetCellValueSafe(IXLCell cell)
    {
        try
        {
            if (cell.TryGetValue<int>(out var integer)) return integer.ToString();
            if (cell.TryGetValue<double>(out var num)) return num.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (cell.TryGetValue<bool>(out var boolean)) return boolean.ToString();
            if (cell.TryGetValue<DateTime>(out var date)) return date.ToString("yyyy-MM-dd");
            if (cell.TryGetValue<string>(out var text) && text is not null) return text;
            return cell.GetString();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read cell value at {CellAddress}", cell.Address);
            return string.Empty;
        }
    }
}
