using Planova.Activity.Domain.Interfaces;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Reporting.Application.DataProviders;

public class WeeklyReportDataProvider : IReportDataProvider<WeeklyReportDataDto>
{
    private readonly IActivityService _activityService;
    private readonly IResourceAssignmentService _assignmentService;
    private readonly IWbsService _wbsService;
    private readonly ILoggingService _logger;

    public WeeklyReportDataProvider(
        IActivityService activityService,
        IResourceAssignmentService assignmentService,
        IWbsService wbsService,
        ILoggingService logger)
    {
        _activityService = activityService;
        _assignmentService = assignmentService;
        _wbsService = wbsService;
        _logger = logger;
    }

    public ReportType HandledType => ReportType.Weekly;

    public async Task<WeeklyReportDataDto> CollectDataAsync(int projectId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        _logger.Info("Collecting weekly report data for project {ProjectId} [{PeriodStart:d} — {PeriodEnd:d}]", projectId, periodStart, periodEnd);

        var weekStart = periodStart.Date;
        var weekEnd = periodEnd.Date;
        var lookAheadEnd = weekEnd.AddDays(7);

        var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
        var assignments = await _assignmentService.GetByProjectAsync(projectId, ct);
        var wbsList = await _wbsService.GetByProjectAsync(projectId, ct);

        var nonSummary = activities.Where(a => a.ActivityType != "WbsSummary").ToList();

        var progressByWbs = new List<WeeklyProgressByWbs>();
        foreach (var wbs in wbsList)
        {
            var tree = await _wbsService.GetTreeAsync(wbs.Id, ct);
            var wbsItems = FlattenTree(tree);

            foreach (var wbsItem in wbsItems)
            {
                var wbsActivities = nonSummary.Where(a => a.WbsItemId == wbsItem.Id).ToList();
                if (wbsActivities.Count == 0) continue;

                var completed = wbsActivities.Count(a => a.Status == "Completed");
                var pct = wbsActivities.Average(a => a.PercentComplete ?? 0m);

                progressByWbs.Add(new WeeklyProgressByWbs(
                    wbsItem.Code,
                    wbsItem.Name,
                    wbsActivities.Count,
                    completed,
                    pct,
                    wbsItem.Weight));
            }
        }

        var resourceUsage = assignments
            .GroupBy(a => new { a.ResourceName, a.ResourceType, a.UnitOfMeasure })
            .Select(g => new WeeklyResourceUsage(
                g.Key.ResourceName,
                g.Key.ResourceType.ToString(),
                g.Sum(a => a.Quantity),
                g.Key.UnitOfMeasure))
            .ToList();

        var today = weekStart;
        var delays = nonSummary
            .Where(a => a.Status != "Completed" && a.PlannedFinish.HasValue && a.PlannedFinish.Value < today)
            .Select(a => new WeeklyDelayItem(
                a.Name,
                a.Code,
                a.PlannedFinish,
                a.ActualFinish,
                a.PlannedFinish.HasValue ? (today - a.PlannedFinish.Value).Days : 0))
            .OrderByDescending(d => d.DelayDays)
            .ToList();

        var lookAhead = nonSummary
            .Where(a => a.PlannedStart.HasValue && a.PlannedStart.Value >= today && a.PlannedStart.Value <= lookAheadEnd)
            .Select(a => new WeeklyLookAheadItem(
                a.Name,
                a.Code,
                a.PlannedStart,
                a.PlannedFinish,
                a.Status))
            .OrderBy(l => l.PlannedStart)
            .ToList();

        var dto = new WeeklyReportDataDto(
            weekStart,
            weekEnd,
            progressByWbs,
            resourceUsage,
            delays,
            lookAhead
        );

        _logger.Info("Weekly report data collected: {WbsCount} WBS items, {ResourceCount} resources, {DelayCount} delays, {LookAheadCount} look-ahead items",
            progressByWbs.Count, resourceUsage.Count, delays.Count, lookAhead.Count);
        return dto;
    }

    private static List<Planova.Wbs.Domain.Entities.WbsItem> FlattenTree(IReadOnlyList<Planova.Wbs.Domain.Entities.WbsItem> items)
    {
        var result = new List<Planova.Wbs.Domain.Entities.WbsItem>();
        foreach (var item in items)
        {
            result.Add(item);
            if (item.Children is { Count: > 0 })
                result.AddRange(FlattenTree(item.Children.ToList()));
        }
        return result;
    }
}
