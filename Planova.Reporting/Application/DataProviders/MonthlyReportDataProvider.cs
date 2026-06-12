using Planova.Activity.Domain.Interfaces;
using Planova.Cost.Domain.Interfaces;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Reporting.Application.DataProviders;

public class MonthlyReportDataProvider : IReportDataProvider<MonthlyReportDataDto>
{
    private readonly IActivityService _activityService;
    private readonly IResourceAssignmentService _assignmentService;
    private readonly IWbsService _wbsService;
    private readonly ICostService _costService;
    private readonly IEvmService _evmService;
    private readonly ILoggingService _logger;

    public MonthlyReportDataProvider(
        IActivityService activityService,
        IResourceAssignmentService assignmentService,
        IWbsService wbsService,
        ICostService costService,
        IEvmService evmService,
        ILoggingService logger)
    {
        _activityService = activityService;
        _assignmentService = assignmentService;
        _wbsService = wbsService;
        _costService = costService;
        _evmService = evmService;
        _logger = logger;
    }

    public ReportType HandledType => ReportType.Monthly;

    public async Task<MonthlyReportDataDto> CollectDataAsync(int projectId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        _logger.Info("Collecting monthly report data for project {ProjectId} [{PeriodStart:d} — {PeriodEnd:d}]", projectId, periodStart, periodEnd);

        var monthStart = periodStart.Date;
        var monthEnd = periodEnd.Date;
        var dataDate = monthEnd;

        var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
        var assignments = await _assignmentService.GetByProjectAsync(projectId, ct);
        var wbsList = await _wbsService.GetByProjectAsync(projectId, ct);

        var nonSummary = activities.Where(a => a.ActivityType != "WbsSummary").ToList();

        // EVM
        MonthlyEvmSummary? evmSummary = null;
        try
        {
            var evm = await _evmService.ComputeMetricsAsync(projectId, dataDate, ct);
            evmSummary = new MonthlyEvmSummary(
                evm.PlannedValue,
                evm.EarnedValue,
                evm.ActualCost,
                evm.CostVariance,
                evm.ScheduleVariance,
                evm.CostPerformanceIndex,
                evm.SchedulePerformanceIndex,
                evm.EstimateAtCompletion,
                evm.BudgetAtCompletion,
                evm.VarianceAtCompletion,
                evm.StatusColor
            );
        }
        catch (Exception ex)
        {
            _logger.Warning("EVM data unavailable for project {ProjectId}: {Message}", projectId, ex.Message);
        }

        // Budget vs Actual
        List<MonthlyBudgetVsActual> budgetVsActual = new();
        try
        {
            var budget = await _costService.GetBudgetAsync(projectId, ct);

            budgetVsActual.Add(new MonthlyBudgetVsActual(
                "Resource Cost",
                budget.ResourceCostTotal,
                0, // Actual resource cost — placeholder for now
                budget.ResourceCostTotal));

            budgetVsActual.Add(new MonthlyBudgetVsActual(
                "Direct Cost",
                budget.DirectCostTotal,
                0, // Actual direct cost — placeholder for now
                budget.DirectCostTotal));

            budgetVsActual.Add(new MonthlyBudgetVsActual(
                "Total Budget",
                budget.TotalBudget,
                0, // Actual total — placeholder for now
                budget.TotalBudget));
        }
        catch (Exception ex)
        {
            _logger.Warning("Budget data unavailable for project {ProjectId}: {Message}", projectId, ex.Message);
        }

        // Progress by WBS
        var progressByWbs = new List<MonthlyProgressByWbs>();
        foreach (var wbs in wbsList)
        {
            var tree = await _wbsService.GetTreeAsync(wbs.Id, ct);
            var flatItems = FlattenTree(tree.ToList());

            foreach (var wbsItem in flatItems)
            {
                var wbsActivities = nonSummary.Where(a => a.WbsItemId == wbsItem.Id).ToList();
                if (wbsActivities.Count == 0) continue;

                var pct = wbsActivities.Average(a => a.PercentComplete ?? 0m);
                progressByWbs.Add(new MonthlyProgressByWbs(
                    wbsItem.Code,
                    wbsItem.Name,
                    pct,
                    wbsItem.Weight));
            }
        }

        // Resource Productivity
        var resourceProductivity = assignments
            .GroupBy(a => new { a.ResourceName, a.ResourceType, a.UnitOfMeasure })
            .Select(g => new MonthlyResourceProductivity(
                g.Key.ResourceName,
                g.Key.ResourceType.ToString(),
                g.Sum(a => a.Quantity),
                g.Sum(a => a.TotalCost),
                g.Key.UnitOfMeasure))
            .ToList();

        var dto = new MonthlyReportDataDto(
            monthStart,
            monthEnd,
            evmSummary,
            budgetVsActual,
            progressByWbs,
            resourceProductivity
        );

        _logger.Info("Monthly report data collected: EVM={EvmAvailable}, BudgetItems={BudgetCount}, WbsProgress={WbsCount}, ResourceItems={ResourceCount}",
            evmSummary != null, budgetVsActual.Count, progressByWbs.Count, resourceProductivity.Count);
        return dto;
    }

    private static List<Planova.Wbs.Domain.Entities.WbsItem> FlattenTree(List<Planova.Wbs.Domain.Entities.WbsItem> items)
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
