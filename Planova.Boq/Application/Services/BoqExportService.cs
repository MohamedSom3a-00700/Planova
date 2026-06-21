using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class BoqExportService : IBoqExportService
{
    private readonly IBoqService _boqService;

    public BoqExportService(IBoqService boqService)
    {
        _boqService = boqService;
    }

    public async Task<ExportResult> ExportToExcelAsync(Guid boqId, ExportOptions options, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var flatItems = FlattenTree(tree);
        var itemCount = flatItems.Count;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(options.SheetName ?? "BOQ");

        var headerStyle = ws.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Description";
        ws.Cell(1, 3).Value = "Unit";
        ws.Cell(1, 4).Value = "Quantity";
        ws.Cell(1, 5).Value = "Rate";
        ws.Cell(1, 6).Value = "Amount";

        for (int col = 1; col <= 6; col++)
        {
            ws.Cell(1, col).Style = headerStyle;
        }

        var row = 2;
        foreach (var item in flatItems)
        {
            ws.Cell(row, 1).Value = item.Code;
            ws.Cell(row, 2).Value = item.Description;
            ws.Cell(row, 3).Value = item.Unit;
            ws.Cell(row, 4).Value = item.Quantity;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = item.Rate;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = item.Amount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        if (options.IncludeGrandTotal)
        {
            ws.Cell(row, 1).Value = "GRAND TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            var total = flatItems.Sum(i => i.Amount);
            ws.Cell(row, 6).Value = total;
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        }

        ws.Columns().AdjustToContents();

        if (!string.IsNullOrEmpty(options.OutputPath))
        {
            wb.SaveAs(options.OutputPath);
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        var data = stream.ToArray();

        return new ExportResult(options.OutputPath, data.Length, itemCount, TimeSpan.Zero, true);
    }

    public async Task<ExportResult> ExportToCsvAsync(Guid boqId, ExportOptions options, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var flatItems = FlattenTree(tree);
        var itemCount = flatItems.Count;

        var csv = new StringBuilder();
        csv.AppendLine("Code,Description,Unit,Quantity,Rate,Amount");
        foreach (var item in flatItems)
        {
            var desc = item.Description?.Contains(',') == true
                ? $"\"{item.Description}\""
                : item.Description ?? string.Empty;
            csv.AppendLine(CultureInfo.InvariantCulture,
                $"{EscapeCsv(item.Code)},{desc},{EscapeCsv(item.Unit)},{item.Quantity},{item.Rate},{item.Amount}");
        }

        if (!string.IsNullOrEmpty(options.OutputPath))
        {
            await File.WriteAllTextAsync(options.OutputPath, csv.ToString(), ct);
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());

        return new ExportResult(options.OutputPath, bytes.Length, itemCount, TimeSpan.Zero, true);
    }

    public async Task<ExportResult> ExportTenderBoqAsync(Guid boqId, ExportOptions options, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var flatItems = FlattenTree(tree);
        var itemCount = flatItems.Count;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(options.SheetName ?? "TenderBOQ");

        var headerStyle = ws.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Description";
        ws.Cell(1, 3).Value = "Unit";
        ws.Cell(1, 4).Value = "Qty";
        ws.Cell(1, 5).Value = "Unit Price";
        ws.Cell(1, 6).Value = "Amount";
        ws.Cell(1, 7).Value = "Rate (Tenderer)";

        for (int col = 1; col <= 7; col++)
            ws.Cell(1, col).Style = headerStyle;

        var row = 2;
        foreach (var item in flatItems)
        {
            ws.Cell(row, 1).Value = item.Code;
            ws.Cell(row, 2).Value = item.Description;
            ws.Cell(row, 3).Value = item.Unit;
            ws.Cell(row, 4).Value = item.Quantity;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = item.Rate;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).FormulaA1 = $"=D{row}*E{row}";
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        if (options.IncludeGrandTotal)
        {
            ws.Cell(row, 1).Value = "GRAND TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 6).FormulaA1 = $"=SUM(F2:F{row - 1})";
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        }

        ws.Columns().AdjustToContents();

        if (!string.IsNullOrEmpty(options.OutputPath))
            wb.SaveAs(options.OutputPath);

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return new ExportResult(options.OutputPath, stream.Length, itemCount, TimeSpan.Zero, true);
    }

    public async Task<ExportResult> ExportClientBoqAsync(Guid boqId, ExportOptions options, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var summaryItems = tree.Where(i => i.ItemType == Domain.Enums.ItemType.Section).ToList();
        var itemCount = summaryItems.Count;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(options.SheetName ?? "ClientBOQ");

        var headerStyle = ws.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Description";
        ws.Cell(1, 3).Value = "Amount";

        for (int col = 1; col <= 3; col++)
            ws.Cell(1, col).Style = headerStyle;

        var row = 2;
        foreach (var item in summaryItems)
        {
            ws.Cell(row, 1).Value = item.Code;
            ws.Cell(row, 2).Value = item.Description;
            ws.Cell(row, 3).Value = item.Amount;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        if (options.IncludeGrandTotal)
        {
            ws.Cell(row, 1).Value = "GRAND TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            var total = summaryItems.Sum(i => i.Amount);
            ws.Cell(row, 3).Value = total;
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        }

        ws.Columns().AdjustToContents();

        if (!string.IsNullOrEmpty(options.OutputPath))
            wb.SaveAs(options.OutputPath);

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return new ExportResult(options.OutputPath, stream.Length, itemCount, TimeSpan.Zero, true);
    }

    public async Task<ExportResult> ExportToPdfAsync(Guid boqId, ExportOptions options, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var flatItems = FlattenTree(tree);
        var itemCount = flatItems.Count;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(options.SheetName ?? "BOQ");

        var headerStyle = ws.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Description";
        ws.Cell(1, 3).Value = "Unit";
        ws.Cell(1, 4).Value = "Qty";
        ws.Cell(1, 5).Value = "Rate";
        ws.Cell(1, 6).Value = "Amount";

        for (int col = 1; col <= 6; col++)
            ws.Cell(1, col).Style = headerStyle;

        var row = 2;
        foreach (var item in flatItems)
        {
            ws.Cell(row, 1).Value = item.Code;
            ws.Cell(row, 2).Value = item.Description;
            ws.Cell(row, 3).Value = item.Unit;
            ws.Cell(row, 4).Value = item.Quantity;
            ws.Cell(row, 5).Value = item.Rate;
            ws.Cell(row, 6).Value = item.Amount;
            row++;
        }

        if (options.IncludeGrandTotal)
        {
            ws.Cell(row, 1).Value = "GRAND TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            var total = flatItems.Sum(i => i.Amount);
            ws.Cell(row, 6).Value = total;
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        }

        ws.Columns().AdjustToContents();

        if (!string.IsNullOrEmpty(options.OutputPath))
            wb.SaveAs(options.OutputPath);

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return new ExportResult(options.OutputPath, stream.Length, itemCount, TimeSpan.Zero, true);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static List<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items)
    {
        var result = new List<BoqItemDto>();
        foreach (var item in items)
        {
            result.Add(item);
            if (item.Children != null)
                result.AddRange(FlattenTree(item.Children));
        }
        return result;
    }
}
