using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public class WbsReportService : IWbsReportService
{
    private readonly IWbsRepository _wbsRepository;
    private readonly IWbsItemRepository _itemRepository;

    public WbsReportService(IWbsRepository wbsRepository, IWbsItemRepository itemRepository)
    {
        _wbsRepository = wbsRepository;
        _itemRepository = itemRepository;
    }

    public async Task<WbsSummaryReport> GenerateSummaryAsync(Guid wbsId, CancellationToken ct)
    {
        var wbs = await _wbsRepository.GetByIdAsync(wbsId, ct);
        var items = await _itemRepository.GetByWbsIdAsync(wbsId, ct);

        var levels = items.Select(i => i.Level).Distinct().OrderBy(l => l).ToList();
        var sections = new List<ReportSection>();
        var totalWeight = items.Sum(i => i.Weight ?? 0);

        foreach (var level in levels)
        {
            var levelItems = items.Where(i => i.Level == level).ToList();
            var levelWeight = levelItems.Sum(i => i.Weight ?? 0);
            var weightPct = totalWeight > 0 ? Math.Round(levelWeight / totalWeight * 100, 1) : 0;

            sections.Add(new ReportSection(
                $"Level {level}",
                levelItems.Count,
                weightPct
            ));
        }

        return new WbsSummaryReport(
            wbsId,
            wbs.Name,
            items.Count,
            levels.Count,
            totalWeight,
            sections
        );
    }

    public async Task<WbsDictionaryReport> GenerateDictionaryAsync(Guid wbsId, CancellationToken ct)
    {
        var wbs = await _wbsRepository.GetByIdAsync(wbsId, ct);
        var items = await _itemRepository.GetByWbsIdAsync(wbsId, ct);

        var entries = items
            .OrderBy(i => i.Level)
            .ThenBy(i => i.SortOrder)
            .Select(i => new DictionaryEntry(
                i.Code,
                i.ShortCode,
                i.Name,
                i.Description,
                i.WbsLevel.ToString(),
                i.AssignedTo,
                i.Deliverable,
                i.SourceBoqItemId
            ))
            .ToList();

        return new WbsDictionaryReport(wbsId, wbs.Name, entries);
    }

    public async Task<byte[]> ExportToExcelAsync(Guid wbsId, ReportType type, CancellationToken ct)
    {
        if (type == ReportType.Summary)
        {
            var report = await GenerateSummaryAsync(wbsId, ct);
            return BuildExcelSummary(report);
        }
        else
        {
            var report = await GenerateDictionaryAsync(wbsId, ct);
            return BuildExcelDictionary(report);
        }
    }

    public async Task<byte[]> ExportToPdfAsync(Guid wbsId, ReportType type, CancellationToken ct)
    {
        if (type == ReportType.Summary)
        {
            var report = await GenerateSummaryAsync(wbsId, ct);
            return BuildPdfSummary(report);
        }
        else
        {
            var report = await GenerateDictionaryAsync(wbsId, ct);
            return BuildPdfDictionary(report);
        }
    }

    private static byte[] BuildExcelSummary(WbsSummaryReport report)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        writer.WriteLine($"WBS Summary Report: {report.WbsName}");
        writer.WriteLine($"Total Items: {report.TotalItems}");
        writer.WriteLine($"Total Levels: {report.TotalLevels}");
        writer.WriteLine($"Total Weight: {report.TotalWeight:F2}%");
        writer.WriteLine();
        writer.WriteLine("Level,Item Count,Weight %");
        foreach (var section in report.Sections)
            writer.WriteLine($"{section.Name},{section.ItemCount},{section.WeightPercentage:F1}");

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildExcelDictionary(WbsDictionaryReport report)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        writer.WriteLine($"WBS Dictionary: {report.WbsName}");
        writer.WriteLine();
        writer.WriteLine("Code,ShortCode,Name,Description,WBS Level,Assigned To,Deliverable,Source BOQ Item");
        foreach (var entry in report.Entries)
            writer.WriteLine($"{entry.Code},{entry.ShortCode},{entry.Name},{entry.Description},{entry.WbsLevel},{entry.AssignedTo},{entry.Deliverable},{entry.SourceBoqItemId}");

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildPdfSummary(WbsSummaryReport report)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        writer.WriteLine("=== WBS Summary Report ===");
        writer.WriteLine($"WBS: {report.WbsName}");
        writer.WriteLine($"Items: {report.TotalItems}  Levels: {report.TotalLevels}  Weight: {report.TotalWeight:F2}%");
        writer.WriteLine(new string('-', 50));
        writer.WriteLine($"{"Level",-20} {"Items",-10} {"Weight%",-10}");
        writer.WriteLine(new string('-', 50));
        foreach (var section in report.Sections)
            writer.WriteLine($"{section.Name,-20} {section.ItemCount,-10} {section.WeightPercentage,-10:F1}");

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildPdfDictionary(WbsDictionaryReport report)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        writer.WriteLine("=== WBS Dictionary ===");
        writer.WriteLine($"WBS: {report.WbsName}");
        writer.WriteLine(new string('=', 70));
        writer.WriteLine($"{"Code",-12} {"Name",-25} {"Level",-15} {"Assigned To",-15}");
        writer.WriteLine(new string('=', 70));
        foreach (var entry in report.Entries)
        {
            var assigned = entry.AssignedTo ?? "";
            var name = entry.Name ?? "";
            writer.WriteLine($"{entry.Code,-12} {Truncate(name, 24),-25} {entry.WbsLevel,-15} {Truncate(assigned, 14),-15}");
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static string Truncate(string value, int maxLength) =>
        string.IsNullOrEmpty(value) ? value : (value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…");
}
