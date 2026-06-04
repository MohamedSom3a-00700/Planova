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
        var itemCount = flatItems.Count();

        if (!string.IsNullOrEmpty(options.OutputPath))
        {
            await File.WriteAllTextAsync(options.OutputPath, $"BOQ Export: {boqId}", ct);
        }

        return new ExportResult(options.OutputPath, 0, itemCount, TimeSpan.Zero, true);
    }

    public async Task<ExportResult> ExportToCsvAsync(Guid boqId, ExportOptions options, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var flatItems = FlattenTree(tree);
        var itemCount = flatItems.Count();

        if (!string.IsNullOrEmpty(options.OutputPath))
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Code,Description,Unit,Quantity,Rate,Amount");
            foreach (var item in FlattenTree(tree))
            {
                csv.AppendLine($"{item.Code},{item.Description},{item.Quantity},{item.Rate},{item.Amount}");
            }
            await File.WriteAllTextAsync(options.OutputPath, csv.ToString(), ct);
        }

        return new ExportResult(options.OutputPath, 0, itemCount, TimeSpan.Zero, true);
    }

    private static List<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items)
    {
        var result = new List<BoqItemDto>();
        foreach (var item in items)
        {
            result.Add(item);
            if (item.Children != null)
            {
                result.AddRange(FlattenTree(item.Children));
            }
        }
        return result;
    }
}
