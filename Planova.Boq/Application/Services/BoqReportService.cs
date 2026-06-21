using System.Text;
using ClosedXML.Excel;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class BoqReportService : IBoqReportService
{
    private readonly IBoqService _boqService;

    public BoqReportService(IBoqService boqService)
    {
        _boqService = boqService;
    }

    public async Task<byte[]> GenerateSummaryReportAsync(Guid boqId, ReportFormat format, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var grandTotal = await _boqService.ComputeSubtotalAsync(boqId, null, ct);

        return format switch
        {
            ReportFormat.Pdf => GeneratePdfSummary(boq, tree, grandTotal),
            ReportFormat.Excel => GenerateExcelSummary(boq, tree, grandTotal),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public async Task<byte[]> GenerateItemizedReportAsync(Guid boqId, ReportFormat format, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);

        return format switch
        {
            ReportFormat.Pdf => GeneratePdfItemized(boq, tree),
            ReportFormat.Excel => GenerateExcelItemized(boq, tree),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public async Task<byte[]> GenerateCostSummaryReportAsync(Guid boqId, ReportFormat format, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);

        return format switch
        {
            ReportFormat.Pdf => GeneratePdfGrouped(boq, tree, "Cost Code", i => i.CostCode ?? "Uncategorized"),
            ReportFormat.Excel => GenerateExcelGrouped(boq, tree, "Cost Code", i => i.CostCode ?? "Uncategorized"),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public async Task<byte[]> GenerateTradeSummaryReportAsync(Guid boqId, ReportFormat format, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);

        return format switch
        {
            ReportFormat.Pdf => GeneratePdfGrouped(boq, tree, "Trade", i => i.Description?.Split(' ').FirstOrDefault() ?? "General"),
            ReportFormat.Excel => GenerateExcelGrouped(boq, tree, "Trade", i => i.Description?.Split(' ').FirstOrDefault() ?? "General"),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public async Task<byte[]> GenerateCsiSummaryReportAsync(Guid boqId, ReportFormat format, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);

        return format switch
        {
            ReportFormat.Pdf => GeneratePdfGrouped(boq, tree, "CSI Division", i => ExtractCsiDivision(i.Code)),
            ReportFormat.Excel => GenerateExcelGrouped(boq, tree, "CSI Division", i => ExtractCsiDivision(i.Code)),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public async Task SaveReportAsync(Guid boqId, ReportType type, ReportFormat format, string outputPath, CancellationToken ct)
    {
        var data = type switch
        {
            ReportType.Summary => await GenerateSummaryReportAsync(boqId, format, ct),
            ReportType.Itemized => await GenerateItemizedReportAsync(boqId, format, ct),
            ReportType.CostSummary => await GenerateCostSummaryReportAsync(boqId, format, ct),
            ReportType.TradeSummary => await GenerateTradeSummaryReportAsync(boqId, format, ct),
            ReportType.CsiSummary => await GenerateCsiSummaryReportAsync(boqId, format, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        await File.WriteAllBytesAsync(outputPath, data, ct);
    }

    private static byte[] GeneratePdfSummary(BoqDto boq, IReadOnlyList<BoqItemDto> tree, decimal grandTotal)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine($"BOQ Summary: {boq.Name}");
        writer.WriteLine($"Currency: {boq.Currency}");
        writer.WriteLine($"Status: {boq.Status}");
        writer.WriteLine($"Total Items: {CountItems(tree)}");
        writer.WriteLine($"Grand Total: {grandTotal:N2}");
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] GeneratePdfItemized(BoqDto boq, IReadOnlyList<BoqItemDto> tree)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine($"BOQ Itemized: {boq.Name}");
        foreach (var item in FlattenTree(tree))
        {
            writer.WriteLine($"{item.Code} | {item.Description} | {item.Quantity:N2} {item.Unit} x {item.Rate:N2} = {item.Amount:N2}");
        }
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] GeneratePdfGrouped(BoqDto boq, IReadOnlyList<BoqItemDto> tree, string groupLabel, Func<BoqItemDto, string> groupKey)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine($"BOQ {groupLabel} Summary: {boq.Name}");

        var groups = FlattenTree(tree)
            .GroupBy(groupKey)
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            writer.WriteLine();
            writer.WriteLine($"{groupLabel}: {group.Key}");
            writer.WriteLine(new string('-', 40));
            foreach (var item in group)
            {
                writer.WriteLine($"{item.Code} | {item.Description} | {item.Amount:N2}");
            }
            writer.WriteLine($"Subtotal: {group.Sum(i => i.Amount):N2}");
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] GenerateExcelSummary(BoqDto boq, IReadOnlyList<BoqItemDto> tree, decimal grandTotal)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Summary");

        ws.Cell(1, 1).Value = "BOQ Summary";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Name";
        ws.Cell(3, 2).Value = boq.Name;
        ws.Cell(4, 1).Value = "Currency";
        ws.Cell(4, 2).Value = boq.Currency;
        ws.Cell(5, 1).Value = "Status";
        ws.Cell(5, 2).Value = boq.Status.ToString();
        ws.Cell(6, 1).Value = "Total Items";
        ws.Cell(6, 2).Value = CountItems(tree);
        ws.Cell(7, 1).Value = "Grand Total";
        ws.Cell(7, 2).Value = grandTotal;
        ws.Cell(7, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(7, 1).Style.Font.Bold = true;
        ws.Cell(7, 2).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GenerateExcelItemized(BoqDto boq, IReadOnlyList<BoqItemDto> tree)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Itemized");

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
        foreach (var item in FlattenTree(tree))
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

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GenerateExcelGrouped(BoqDto boq, IReadOnlyList<BoqItemDto> tree, string groupLabel, Func<BoqItemDto, string> groupKey)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add($"{groupLabel} Summary");

        var headerStyle = ws.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell(1, 1).Value = $"{groupLabel}";
        ws.Cell(1, 2).Value = "Code";
        ws.Cell(1, 3).Value = "Description";
        ws.Cell(1, 4).Value = "Amount";

        for (int col = 1; col <= 4; col++)
            ws.Cell(1, col).Style = headerStyle;

        var groups = FlattenTree(tree)
            .GroupBy(groupKey)
            .OrderBy(g => g.Key);

        var row = 2;
        foreach (var group in groups)
        {
            foreach (var item in group)
            {
                ws.Cell(row, 1).Value = group.Key;
                ws.Cell(row, 2).Value = item.Code;
                ws.Cell(row, 3).Value = item.Description;
                ws.Cell(row, 4).Value = item.Amount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                row++;
            }
            ws.Cell(row, 1).Value = "Subtotal";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = group.Sum(i => i.Amount);
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static string ExtractCsiDivision(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "Uncategorized";
        var parts = code.Split(new[] { '.', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "Uncategorized";
    }

    private static int CountItems(IReadOnlyList<BoqItemDto> items) =>
        items.Count + items.Sum(i => i.Children != null ? CountItems(i.Children) : 0);

    private static IEnumerable<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items)
    {
        foreach (var item in items)
        {
            yield return item;
            if (item.Children != null)
            {
                foreach (var child in FlattenTree(item.Children))
                    yield return child;
            }
        }
    }
}
