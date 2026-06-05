using ClosedXML.Excel;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class ResourceHistogramService : IResourceHistogramService
{
    private readonly IResourceUsageRepository _usageRepository;
    private readonly IResourceRepository _resourceRepository;

    public ResourceHistogramService(IResourceUsageRepository usageRepository, IResourceRepository resourceRepository)
    {
        _usageRepository = usageRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<ResourceHistogramDto> GetHistogramAsync(int projectId, HistogramFilter filter, CancellationToken ct = default)
    {
        var usages = await _usageRepository.GetByProjectAsync(
            projectId, filter.From, filter.To, filter.ResourceType, ct);

        if (filter.ResourceId.HasValue)
            usages = usages.Where(u => u.ResourceId == filter.ResourceId.Value).ToList();

        var groupedByDate = usages
            .GroupBy(u => u.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var dailyData = new List<HistogramDayDto>();
        foreach (var group in groupedByDate)
        {
            var breakdown = group.Select(u =>
            {
                var resource = usages.FirstOrDefault(x => x.ResourceId == u.ResourceId);
                return new HistogramBreakdownItem
                {
                    ResourceId = u.ResourceId,
                    Quantity = u.PlannedQuantity,
                };
            }).GroupBy(b => b.ResourceId).Select(g => new HistogramBreakdownItem
            {
                ResourceId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            }).ToList();

            var totalQuantity = group.Sum(u => u.PlannedQuantity);
            dailyData.Add(new HistogramDayDto
            {
                Date = group.Key,
                TotalQuantity = totalQuantity,
                Breakdown = breakdown
            });
        }

        var resourceSummaries = usages
            .GroupBy(u => u.ResourceId)
            .Select(g => new HistogramResourceSummary
            {
                ResourceId = g.Key,
                TotalQuantity = g.Sum(u => u.PlannedQuantity)
            }).ToList();

        return new ResourceHistogramDto
        {
            DailyData = dailyData,
            ResourceSummaries = resourceSummaries,
            ProjectStart = filter.From ?? (dailyData.Count > 0 ? dailyData.Min(d => d.Date) : DateTime.Today),
            ProjectEnd = filter.To ?? (dailyData.Count > 0 ? dailyData.Max(d => d.Date) : DateTime.Today),
            TotalDays = dailyData.Count,
            AppliedFilter = filter
        };
    }

    public async Task<List<ResourceHistogramDto>> GetByResourceTypeAsync(int projectId, ResourceType type, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var filter = new HistogramFilter
        {
            From = from,
            To = to,
            ResourceType = type,
            Aggregation = HistogramAggregation.Sum
        };
        var result = await GetHistogramAsync(projectId, filter, ct);
        return [result];
    }

    public async Task<List<ResourceHistogramDto>> GetByResourceAsync(int projectId, Guid resourceId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var filter = new HistogramFilter
        {
            From = from,
            To = to,
            ResourceId = resourceId,
            Aggregation = HistogramAggregation.Sum
        };
        var result = await GetHistogramAsync(projectId, filter, ct);
        return [result];
    }

    public async Task<byte[]> ExportHistogramDataAsync(int projectId, HistogramFilter filter, CancellationToken ct = default)
    {
        var histogram = await GetHistogramAsync(projectId, filter, ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Histogram");

        ws.Cell(1, 1).Value = "Date";
        ws.Cell(1, 2).Value = "Total Quantity";
        ws.Cell(1, 3).Value = "Is Overallocated";

        int row = 2;
        foreach (var day in histogram.DailyData)
        {
            ws.Cell(row, 1).Value = day.Date.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = (double)day.TotalQuantity;
            ws.Cell(row, 3).Value = day.IsOverallocated ? "Yes" : "No";
            row++;

            foreach (var item in day.Breakdown)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = (double)item.Quantity;
                ws.Cell(row, 3).Value = "";
                row++;
            }
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
