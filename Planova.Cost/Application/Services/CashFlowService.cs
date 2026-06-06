using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Activity.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Cost.Application.Services;

public class CashFlowService : ICashFlowService
{
    private readonly IDirectCostRepository _directCostRepository;
    private readonly IActualCostRepository _actualCostRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IResourceAssignmentRepository _resourceAssignmentRepository;
    private readonly ICostBaselineRepository _baselineRepository;

    public CashFlowService(
        IDirectCostRepository directCostRepository,
        IActualCostRepository actualCostRepository,
        IActivityRepository activityRepository,
        IResourceAssignmentRepository resourceAssignmentRepository,
        ICostBaselineRepository baselineRepository)
    {
        _directCostRepository = directCostRepository;
        _actualCostRepository = actualCostRepository;
        _activityRepository = activityRepository;
        _resourceAssignmentRepository = resourceAssignmentRepository;
        _baselineRepository = baselineRepository;
    }

    public async Task<List<CashFlowPeriodDto>> GetCashFlowAsync(
        int projectId, CashFlowPeriodType periodType, DateTime? dataDate, CancellationToken ct = default)
    {
        var now = dataDate ?? DateTime.UtcNow;
        var start = now.AddMonths(-6);
        var end = now.AddMonths(12);

        var activities = await _activityRepository.GetByProjectIdAsync(projectId, ct);
        var assignments = await _resourceAssignmentRepository.GetByProjectAsync(projectId, ct);
        var directCosts = await _directCostRepository.GetByProjectIdAsync(projectId, ct);
        var actualCosts = await _actualCostRepository.GetByProjectIdAsync(projectId, ct);

        var periods = GeneratePeriods(start, end, periodType);

        var plannedByPeriod = DistributePlannedCosts(activities, assignments, directCosts, periods);
        var actualByPeriod = DistributeActualCosts(actualCosts, periods);

        var cumPlanned = 0m;
        var cumActual = 0m;
        var result = new List<CashFlowPeriodDto>();

        foreach (var period in periods)
        {
            var planned = plannedByPeriod.TryGetValue(period.Start, out var p) ? p : 0m;
            var actual = actualByPeriod.TryGetValue(period.Start, out var a) ? a : 0m;
            cumPlanned += planned;
            cumActual += actual;
            result.Add(new CashFlowPeriodDto(period.Start, period.End, planned, actual, cumPlanned, cumActual));
        }

        return result;
    }

    private static List<(DateTime Start, DateTime End)> GeneratePeriods(DateTime start, DateTime end, CashFlowPeriodType periodType)
    {
        var periods = new List<(DateTime Start, DateTime End)>();
        var current = start;

        while (current < end)
        {
            var periodEnd = periodType == CashFlowPeriodType.Weekly
                ? current.AddDays(7)
                : current.AddMonths(1);

            if (periodEnd > end)
                periodEnd = end;

            periods.Add((current, periodEnd));
            current = periodEnd;
        }

        return periods;
    }

    private static Dictionary<DateTime, decimal> DistributePlannedCosts(
        IReadOnlyList<Planova.Activity.Domain.Entities.Activity> activities,
        IReadOnlyList<Planova.Resource.Domain.Entities.ResourceAssignment> assignments,
        IReadOnlyList<Planova.Cost.Domain.Entities.DirectCost> directCosts,
        List<(DateTime Start, DateTime End)> periods)
    {
        var costByPeriod = periods.ToDictionary(p => p.Start, _ => 0m);

        var costByActivity = new Dictionary<Guid, decimal>();
        foreach (var a in assignments)
        {
            costByActivity.TryGetValue(a.ActivityId, out var existing);
            costByActivity[a.ActivityId] = existing + a.TotalCost;
        }
        foreach (var d in directCosts)
        {
            if (d.ActivityId.HasValue)
            {
                costByActivity.TryGetValue(d.ActivityId.Value, out var existing);
                costByActivity[d.ActivityId.Value] = existing + d.TotalAmount;
            }
        }

        foreach (var activity in activities)
        {
            if (!costByActivity.TryGetValue(activity.Id, out var totalCost) || totalCost == 0)
                continue;

            var pStart = activity.PlannedStart ?? periods[0].Start;
            var pEnd = activity.PlannedFinish ?? periods[^1].End;

            if (pStart >= pEnd) pEnd = pStart.AddMonths(1);

            var relevantPeriods = periods
                .Where(p => p.Start < pEnd && p.End > pStart)
                .ToList();

            if (relevantPeriods.Count == 0) continue;

            var costPerPeriod = totalCost / relevantPeriods.Count;
            foreach (var rp in relevantPeriods)
            {
                costByPeriod[rp.Start] += costPerPeriod;
            }
        }

        return costByPeriod;
    }

    private static Dictionary<DateTime, decimal> DistributeActualCosts(
        IReadOnlyList<Planova.Cost.Domain.Entities.ActualCost> actualCosts,
        List<(DateTime Start, DateTime End)> periods)
    {
        var costByPeriod = periods.ToDictionary(p => p.Start, _ => 0m);

        foreach (var ac in actualCosts)
        {
            var period = periods.FirstOrDefault(p => ac.EntryDate >= p.Start && ac.EntryDate < p.End);
            if (period != default)
            {
                costByPeriod[period.Start] += ac.Amount;
            }
            else
            {
                var lastPeriod = periods[^1];
                if (ac.EntryDate >= lastPeriod.Start)
                    costByPeriod[lastPeriod.Start] += ac.Amount;
                else
                    costByPeriod[periods[0].Start] += ac.Amount;
            }
        }

        return costByPeriod;
    }
}
