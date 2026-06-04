using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;
using Planova.Excel.Readers;

namespace Planova.UI.Services;

public class ExcelRowReader : IExcelRowReader
{
    private readonly IWorkbookReader _workbookReader;

    public ExcelRowReader(IWorkbookReader workbookReader)
    {
        _workbookReader = workbookReader;
    }

    public async Task<IReadOnlyList<string>> GetWorksheetsAsync(string filePath, CancellationToken ct)
    {
        var info = await _workbookReader.OpenAsync(filePath, ct);
        return info.Worksheets.Select(w => w.Name).ToList();
    }

    public async Task<IReadOnlyList<ImportRow>> ReadAsync(string filePath, CancellationToken ct)
    {
        return await ReadAsync(filePath, null, ct);
    }

    public async Task<IReadOnlyList<ImportRow>> ReadAsync(string filePath, string? sheetName, CancellationToken ct)
    {
        IReadOnlyList<Dictionary<string, object>> rawData;

        if (!string.IsNullOrEmpty(sheetName))
        {
            rawData = await _workbookReader.ReadAllAsync(filePath, sheetName, ct);
        }
        else
        {
            var info = await _workbookReader.OpenAsync(filePath, ct);
            var name = info.Worksheets.FirstOrDefault()?.Name;
            if (string.IsNullOrEmpty(name))
                return Array.Empty<ImportRow>();
            rawData = await _workbookReader.ReadAllAsync(filePath, name, ct);
        }

        return MapToImportRows(rawData);
    }

    private static IReadOnlyList<ImportRow> MapToImportRows(IReadOnlyList<Dictionary<string, object>> rawData)
    {
        if (rawData.Count == 0) return Array.Empty<ImportRow>();

        var columns = rawData[0].Keys.ToList();
        var codeCol = FindColumn(columns,
            "Code", "Item Code", "ItemCode", "item_code", "ID", "Id", "Ref", "Reference",
            "Number", "No", "Item No", "ItemNo", "Item #", "Item#");
        var descCol = FindColumn(columns,
            "Description", "Desc", "Item Description", "ItemDescription", "description",
            "Name", "Item Name", "ItemName", "Title", "Work Item", "WorkItem",
            "Scope of Work", "ScopeOfWork", "Scope");
        var unitCol = FindColumn(columns, "Unit", "UOM", "Unit of Measure", "unit_of_measure");
        var qtyCol = FindColumn(columns, "Quantity", "Qty", "QTY", "quantity", "qty");
        var rateCol = FindColumn(columns, "Rate", "Unit Rate", "UnitRate", "unit_rate", "rate",
            "Unit Price", "UnitPrice", "Price");
        var levelCol = FindColumn(columns, "Level", "level", "WBS Level", "wbs_level");
        var parentCodeCol = FindColumn(columns, "ParentCode", "Parent Code", "ParentCode", "parent_code");
        var parentIdCol = FindColumn(columns, "ParentId", "Parent ID", "ParentId", "parent_id");
        var rows = new List<ImportRow>(rawData.Count);

        foreach (var row in rawData)
        {
            var rawValues = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);

            var code = GetString(row, codeCol);
            var description = GetString(row, descCol);
            var unit = unitCol != null ? GetString(row, unitCol) : "EA";
            var qty = GetDecimal(row, qtyCol);
            var rate = GetDecimal(row, rateCol);
            int? level = levelCol != null ? GetInt(row, levelCol) : null;
            string? parentCode = parentCodeCol != null ? GetString(row, parentCodeCol) : null;
            string? parentId = parentIdCol != null ? GetString(row, parentIdCol) : null;

            rows.Add(new ImportRow(
                code, description, unit, qty, rate,
                level, parentId, parentCode,
                rawValues
            ));
        }

        return rows;
    }

    private static string? FindColumn(IReadOnlyList<string> columns, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var match = columns.FirstOrDefault(c =>
                string.Equals(c.Trim(), candidate, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }
        return null;
    }

    private static string GetString(Dictionary<string, object> row, string? column)
    {
        if (column == null || !row.TryGetValue(column, out var val)) return string.Empty;
        if (val == null) return string.Empty;
        return val.ToString()?.Trim() ?? string.Empty;
    }

    private static decimal GetDecimal(Dictionary<string, object> row, string? column)
    {
        if (column == null || !row.TryGetValue(column, out var val)) return 0m;
        if (val is decimal d) return d;
        if (val is double db) return (decimal)db;
        if (val is int i) return i;
        if (val is float f) return (decimal)f;
        if (val is long l) return l;
        if (val is string s && decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed)) return parsed;
        return 0m;
    }

    private static int? GetInt(Dictionary<string, object> row, string? column)
    {
        if (column == null || !row.TryGetValue(column, out var val)) return null;
        if (val is int i) return i;
        if (val is double d) return (int)d;
        if (val is decimal dm) return (int)dm;
        if (val is long l) return (int)l;
        if (val is string s && int.TryParse(s, out var parsed)) return parsed;
        return null;
    }
}
