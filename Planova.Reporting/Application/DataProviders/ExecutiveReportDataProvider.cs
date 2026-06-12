using Planova.Activity.Domain.Interfaces;
using Planova.Cost.Domain.Interfaces;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Reporting.Application.DataProviders;

public class ExecutiveReportDataProvider : IReportDataProvider<ExecutiveReportDataDto>
{
    private readonly IActivityService _activityService;
    private readonly IEvmService _evmService;
    private readonly ICostService _costService;
    private readonly IResourceAssignmentService _assignmentService;
    private readonly IProjectPartyService _partyService;
    private readonly ILoggingService _logger;

    public ExecutiveReportDataProvider(
        IActivityService activityService,
        IEvmService evmService,
        ICostService costService,
        IResourceAssignmentService assignmentService,
        IProjectPartyService partyService,
        ILoggingService logger)
    {
        _activityService = activityService;
        _evmService = evmService;
        _costService = costService;
        _assignmentService = assignmentService;
        _partyService = partyService;
        _logger = logger;
    }

    public ReportType HandledType => ReportType.Executive;

    public async Task<ExecutiveReportDataDto> CollectDataAsync(int projectId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        _logger.Info("Collecting executive report data for project {ProjectId} [{PeriodStart:d} — {PeriodEnd:d}]", projectId, periodStart, periodEnd);

        var dataDate = periodEnd;

        var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
        var assignments = await _assignmentService.GetByProjectAsync(projectId, ct);

        // KPI Cards
        var kpiCards = new List<ExecutiveKpiCard>();
        ExecutiveFinancialOverview? financialOverview = null;

        try
        {
            var evm = await _evmService.ComputeMetricsAsync(projectId, dataDate, ct);

            kpiCards.Add(new ExecutiveKpiCard("Planned Value (PV)", evm.PlannedValue, evm.StatusColor switch { "Green" => "$", "Amber" => "$", _ => "$" }, null, evm.StatusColor));
            kpiCards.Add(new ExecutiveKpiCard("Earned Value (EV)", evm.EarnedValue, "$", null, evm.StatusColor));
            kpiCards.Add(new ExecutiveKpiCard("Actual Cost (AC)", evm.ActualCost, "$", null, evm.StatusColor));
            kpiCards.Add(new ExecutiveKpiCard("CPI", evm.CostPerformanceIndex ?? 0, "", evm.CostPerformanceIndex >= 1 ? "On Track" : "Over Budget", evm.CostPerformanceIndex >= 1 ? "Green" : "Red"));
            kpiCards.Add(new ExecutiveKpiCard("SPI", evm.SchedulePerformanceIndex ?? 0, "", evm.SchedulePerformanceIndex >= 1 ? "On Track" : "Behind", evm.SchedulePerformanceIndex >= 1 ? "Green" : "Red"));
            kpiCards.Add(new ExecutiveKpiCard("EAC", evm.EstimateAtCompletion, "$", null, evm.StatusColor));
            kpiCards.Add(new ExecutiveKpiCard("VAC", evm.VarianceAtCompletion, "$", evm.VarianceAtCompletion >= 0 ? "Favourable" : "Unfavourable", evm.VarianceAtCompletion >= 0 ? "Green" : "Red"));

            financialOverview = new ExecutiveFinancialOverview(
                evm.BudgetAtCompletion,
                evm.ActualCost,
                evm.CostVariance,
                evm.EstimateAtCompletion,
                evm.VarianceAtCompletion,
                evm.CostPerformanceIndex
            );
        }
        catch (Exception ex)
        {
            _logger.Warning("EVM data unavailable for executive report on project {ProjectId}: {Message}", projectId, ex.Message);
        }

        // S-Curve — generate monthly data points over the period
        var sCurve = new List<ExecutiveSCurvePoint>();
        var current = new DateTime(periodStart.Year, periodStart.Month, 1);
        while (current <= periodEnd)
        {
            var monthEnd = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
            if (monthEnd > periodEnd) monthEnd = periodEnd;

            try
            {
                var monthEvm = await _evmService.ComputeMetricsAsync(projectId, monthEnd, ct);
                sCurve.Add(new ExecutiveSCurvePoint(
                    monthEnd,
                    monthEvm.PlannedValue,
                    monthEvm.EarnedValue,
                    monthEvm.ActualCost
                ));
            }
            catch
            {
                sCurve.Add(new ExecutiveSCurvePoint(monthEnd, 0, 0, 0));
            }

            current = current.AddMonths(1);
        }

        // Milestones — activities with WbsSummary type or key activities
        var milestones = activities
            .Where(a => a.ActivityType == "WbsSummary" || (a.ActivityType == "Task"))
            .Select(a => new ExecutiveMilestoneItem(
                a.Name,
                a.Code,
                a.Status,
                a.PlannedStart,
                a.PlannedFinish,
                a.ActualStart,
                a.ActualFinish,
                a.PercentComplete ?? 0
            ))
            .OrderBy(m => m.PlannedStart)
            .ToList();

        // Project Parties
        var projectParties = new List<ExecutiveProjectParty>();
        try
        {
            var parties = await _partyService.GetPartiesAsync(projectId, ct);
            foreach (var party in parties)
            {
                projectParties.Add(new ExecutiveProjectParty(
                    party.Name,
                    party.LogoPath,
                    party.Role
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.Warning("Project parties unavailable for project {ProjectId}: {Message}", projectId, ex.Message);
        }

        var dto = new ExecutiveReportDataDto(
            periodStart,
            periodEnd,
            kpiCards,
            sCurve,
            financialOverview,
            milestones,
            projectParties
        );

        _logger.Info("Executive report data collected: {KpiCount} KPIs, {SCurveCount} S-Curve points, {MilestoneCount} milestones, {PartyCount} parties",
            kpiCards.Count, sCurve.Count, milestones.Count, projectParties.Count);
        return dto;
    }
}
