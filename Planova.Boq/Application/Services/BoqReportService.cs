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

    public async Task SaveReportAsync(Guid boqId, ReportType type, ReportFormat format, string outputPath, CancellationToken ct)
    {
        var data = type switch
        {
            ReportType.Summary => await GenerateSummaryReportAsync(boqId, format, ct),
            ReportType.Itemized => await GenerateItemizedReportAsync(boqId, format, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        await File.WriteAllBytesAsync(outputPath, data, ct);
    }

    private static byte[] GeneratePdfSummary(BoqDto boq, IReadOnlyList<BoqItemDto> tree, decimal grandTotal)
    {
        using var stream = new MemoryStream();
        var writer = new System.IO.StreamWriter(stream);
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
        var writer = new System.IO.StreamWriter(stream);
        writer.WriteLine($"BOQ Itemized: {boq.Name}");
        foreach (var item in FlattenTree(tree))
        {
            writer.WriteLine($"{item.Code} | {item.Description} | {item.Quantity:N2} {item.Unit} x {item.Rate:N2} = {item.Amount:N2}");
        }
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] GenerateExcelSummary(BoqDto boq, IReadOnlyList<BoqItemDto> tree, decimal grandTotal) =>
        "Excel summary placeholder"u8.ToArray();

    private static byte[] GenerateExcelItemized(BoqDto boq, IReadOnlyList<BoqItemDto> tree) =>
        "Excel itemized placeholder"u8.ToArray();

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
                {
                    yield return child;
                }
            }
        }
    }
}
