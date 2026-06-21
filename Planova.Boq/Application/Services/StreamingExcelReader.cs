using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Xml;
using Planova.Boq.Application.Dto;

namespace Planova.Boq.Application.Services;

public sealed class StreamingExcelReader : IDisposable
{
    private const string SpreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const long StreamingThreshold = 10 * 1024 * 1024;

    private readonly ZipArchive _archive;
    private readonly Dictionary<int, string> _sharedStrings;
    private readonly List<string> _sheetNames;
    private readonly List<string> _sheetFiles;
    private bool _disposed;

    private StreamingExcelReader(ZipArchive archive, Dictionary<int, string> sharedStrings, List<string> sheetNames, List<string> sheetFiles)
    {
        _archive = archive;
        _sharedStrings = sharedStrings;
        _sheetNames = sheetNames;
        _sheetFiles = sheetFiles;
    }

    public static async Task<StreamingExcelReader> OpenAsync(string filePath, CancellationToken ct)
    {
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        try
        {
            var sharedStrings = await ReadSharedStringsAsync(archive, ct);
            var (names, files) = await ReadWorkbookAsync(archive, ct);
            return new StreamingExcelReader(archive, sharedStrings, names, files);
        }
        catch
        {
            archive.Dispose();
            stream.Dispose();
            throw;
        }
    }

    public static bool ShouldStream(string filePath)
    {
        try
        {
            var info = new FileInfo(filePath);
            return info.Exists && info.Length > StreamingThreshold;
        }
        catch
        {
            return false;
        }
    }

    public IReadOnlyList<string> SheetNames => _sheetNames;

    public async IAsyncEnumerable<ImportRow> ReadSheetAsync(int sheetIndex, [EnumeratorCancellation] CancellationToken ct)
    {
        if (sheetIndex < 0 || sheetIndex >= _sheetFiles.Count)
            yield break;

        var entry = _archive.GetEntry(_sheetFiles[sheetIndex]);
        if (entry is null)
            yield break;

        await using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreWhitespace = true });

        await reader.MoveToContentAsync();
        var headerMapping = new Dictionary<int, string>();
        var rowNum = 0;

        while (await reader.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "row")
                continue;

            var depth = reader.Depth;
            var rowCells = new Dictionary<int, object>();
            var rAttr = reader.GetAttribute("r");
            int.TryParse(rAttr, out rowNum);

            if (!reader.ReadToDescendant("c", SpreadsheetNs))
                continue;

            do
            {
                if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "c")
                    continue;

                var cellRef = reader.GetAttribute("r") ?? string.Empty;
                var cellType = reader.GetAttribute("t") ?? string.Empty;
                var colIndex = CellRefToColumnIndex(cellRef);
                if (colIndex < 0) continue;

                var value = await ReadCellValueAsync(reader, cellType, ct);
                if (value is not null)
                    rowCells[colIndex] = value;

            } while (reader.Read() && reader.Depth > depth);

            if (rowNum == 1)
            {
                headerMapping = BuildHeaderMapping(rowCells);
                continue;
            }

            if (headerMapping.Count == 0)
                continue;

            var code = GetValue(rowCells, headerMapping, ["Code", "Item Code", "ItemCode", "ID", "Ref", "Number", "No"]);
            var description = GetValue(rowCells, headerMapping, ["Description", "Desc", "Name", "Title"]);
            var unit = GetValue(rowCells, headerMapping, ["Unit", "UOM"]) ?? "EA";
            var qty = GetDecimalValue(rowCells, headerMapping, ["Quantity", "Qty"]);
            var rate = GetDecimalValue(rowCells, headerMapping, ["Rate", "Unit Rate", "UnitPrice", "Price"]);
            var level = GetIntValue(rowCells, headerMapping, ["Level", "WBS Level"]);
            var parentCode = GetValue(rowCells, headerMapping, ["ParentCode", "Parent Code"]);

            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(description))
                continue;

            yield return new ImportRow(
                code ?? string.Empty,
                description ?? string.Empty,
                unit,
                qty,
                rate,
                level,
                null,
                parentCode,
                null,
                null,
                null,
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            );
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _archive.Dispose();
            _disposed = true;
        }
    }

    private static async Task<Dictionary<int, string>> ReadSharedStringsAsync(ZipArchive archive, CancellationToken ct)
    {
        var result = new Dictionary<int, string>();
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null) return result;

        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreWhitespace = true });
        await reader.MoveToContentAsync();
        var index = 0;

        while (await reader.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "si")
            {
                var text = await ReadElementTextAsync(reader, ct);
                result[index++] = text;
            }
        }

        return result;
    }

    private static async Task<(List<string> Names, List<string> Files)> ReadWorkbookAsync(ZipArchive archive, CancellationToken ct)
    {
        var names = new List<string>();
        var files = new List<string>();

        var entry = archive.GetEntry("xl/workbook.xml");
        if (entry is null) return (names, files);

        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreWhitespace = true });
        await reader.MoveToContentAsync();

        while (await reader.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "sheet")
            {
                var name = reader.GetAttribute("name") ?? $"Sheet{names.Count + 1}";
                names.Add(name);
            }
        }

        for (int i = 0; i < names.Count; i++)
            files.Add($"xl/worksheets/sheet{i + 1}.xml");

        return (names, files);
    }

    private static async Task<string> ReadElementTextAsync(XmlReader reader, CancellationToken ct)
    {
        if (!reader.ReadToDescendant("t", SpreadsheetNs))
            return string.Empty;

        return await reader.ReadElementContentAsStringAsync();
    }

    private static async Task<object?> ReadCellValueAsync(XmlReader reader, string cellType, CancellationToken ct)
    {
        if (!reader.ReadToDescendant("v", SpreadsheetNs) && !reader.ReadToDescendant("is", SpreadsheetNs))
            return null;

        if (reader.LocalName == "is")
        {
            if (reader.ReadToDescendant("t", SpreadsheetNs))
                return await reader.ReadElementContentAsStringAsync();
            return null;
        }

        var value = await reader.ReadElementContentAsStringAsync();
        if (string.IsNullOrEmpty(value))
            return null;

        return cellType switch
        {
            "s" => int.TryParse(value, out var si) ? si : value,
            "b" => value == "1",
            _ => value
        };
    }

    private static int CellRefToColumnIndex(string cellRef)
    {
        if (string.IsNullOrEmpty(cellRef))
            return -1;

        var colPart = cellRef.TrimEnd("0123456789".ToCharArray());
        if (string.IsNullOrEmpty(colPart))
            return -1;

        var index = 0;
        foreach (var ch in colPart)
        {
            if (ch < 'A' || ch > 'Z')
                return -1;
            index = index * 26 + (ch - 'A' + 1);
        }

        return index - 1;
    }

    private static Dictionary<int, string> BuildHeaderMapping(Dictionary<int, object> headerCells)
    {
        var mapping = new Dictionary<int, string>();
        foreach (var kvp in headerCells)
        {
            var text = kvp.Value?.ToString()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(text))
                mapping[kvp.Key] = text;
        }
        return mapping;
    }

    private string? GetValue(Dictionary<int, object> cells, Dictionary<int, string> headerMapping, string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var col = headerMapping.FirstOrDefault(h =>
                string.Equals(h.Value, candidate, StringComparison.OrdinalIgnoreCase));

            if (cells.TryGetValue(col.Key, out var val) && val is not null)
            {
                var text = val is int si && _sharedStrings.TryGetValue(si, out var resolved) ? resolved : val.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }
        return null;
    }

    private static decimal GetDecimalValue(Dictionary<int, object> cells, Dictionary<int, string> headerMapping, string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var col = headerMapping.FirstOrDefault(h =>
                string.Equals(h.Value, candidate, StringComparison.OrdinalIgnoreCase));

            if (cells.TryGetValue(col.Key, out var val) && val is not null)
            {
                var text = val is int si ? si.ToString(CultureInfo.InvariantCulture) : val.ToString();
                if (text is not null && decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d;
            }
        }
        return 0m;
    }

    private static int? GetIntValue(Dictionary<int, object> cells, Dictionary<int, string> headerMapping, string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var col = headerMapping.FirstOrDefault(h =>
                string.Equals(h.Value, candidate, StringComparison.OrdinalIgnoreCase));

            if (cells.TryGetValue(col.Key, out var val) && val is not null)
            {
                var text = val is int si ? si.ToString(CultureInfo.InvariantCulture) : val.ToString();
                if (text is not null && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    return i;
            }
        }
        return null;
    }
}
