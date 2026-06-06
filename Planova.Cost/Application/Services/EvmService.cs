using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Activity.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Cost.Application.Services;

public class EvmService : IEvmService
{
    private readonly ICostBaselineRepository _baselineRepository;
    private readonly IActualCostRepository _actualCostRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IResourceAssignmentRepository _resourceAssignmentRepository;
    private readonly IDirectCostRepository _directCostRepository;

    public EvmService(
        ICostBaselineRepository baselineRepository,
        IActualCostRepository actualCostRepository,
        IActivityRepository activityRepository,
        IResourceAssignmentRepository resourceAssignmentRepository,
        IDirectCostRepository directCostRepository)
    {
        _baselineRepository = baselineRepository;
        _actualCostRepository = actualCostRepository;
        _activityRepository = activityRepository;
        _resourceAssignmentRepository = resourceAssignmentRepository;
        _directCostRepository = directCostRepository;
    }

    public async Task<EvmMetricsDto> ComputeMetricsAsync(int projectId, DateTime dataDate, CancellationToken ct = default)
    {
        var baseline = await _baselineRepository.GetActiveBaselineAsync(projectId, ct);
        var activities = await _activityRepository.GetByProjectIdAsync(projectId, ct);
        var actualCosts = await _actualCostRepository.GetByProjectIdAsync(projectId, ct);
        var assignments = await _resourceAssignmentRepository.GetByProjectAsync(projectId, ct);
        var directCosts = await _directCostRepository.GetByProjectIdAsync(projectId, ct);

        var totalActualCost = actualCosts.Sum(a => a.Amount);
        var totalAssignmentCost = assignments.Sum(a => a.TotalCost);
        var totalDirectCost = directCosts.Sum(d => d.TotalAmount);

        var bac = baseline?.Rows.Sum(r => r.BudgetAtCompletion) ?? totalAssignmentCost + totalDirectCost;

        var pv = 0m;
        var ev = 0m;

        foreach (var activity in activities)
        {
            var plannedStart = activity.PlannedStart;
            var plannedFinish = activity.PlannedFinish;
            var percentComplete = activity.PercentComplete ?? 0m;

            var resCost = 0m;
            foreach (var a in assignments)
            {
                if (a.ActivityId == activity.Id)
                    resCost += a.TotalCost;
            }
            var dcCost = 0m;
            foreach (var d in directCosts)
            {
                if (d.ActivityId == activity.Id)
                    dcCost += d.TotalAmount;
            }
            var totalPlannedForActivity = resCost + dcCost;

            if (totalPlannedForActivity == 0) continue;

            if (plannedStart.HasValue && plannedFinish.HasValue && plannedStart.Value < plannedFinish.Value)
            {
                var totalDuration = (plannedFinish.Value - plannedStart.Value).TotalDays;
                var elapsed = (dataDate - plannedStart.Value).TotalDays;

                if (elapsed <= 0)
                {
                }
                else if (elapsed >= totalDuration)
                {
                    pv += totalPlannedForActivity;
                }
                else
                {
                    pv += totalPlannedForActivity * (decimal)(elapsed / totalDuration);
                }
            }
            else
            {
                pv += totalPlannedForActivity;
            }

            ev += totalPlannedForActivity * (percentComplete / 100m);
        }

        var ac = totalActualCost;
        var cv = ev - ac;
        var sv = ev - pv;
        decimal? cpi = ac > 0 ? ev / ac : null;
        decimal? spi = pv > 0 ? ev / pv : null;
        var eac = cpi.HasValue && cpi.Value > 0 ? bac / cpi.Value : bac;
        var etc = eac - ac;
        var vac = bac - eac;
        var statusColor = (cpi ?? 1) >= 1.0m ? "Green" : (cpi ?? 1) >= 0.8m ? "Amber" : "Red";

        return new EvmMetricsDto(
            dataDate, pv, ev, ac, cv, sv, cpi, spi, eac, etc, vac, bac, statusColor);
    }

    public async Task<List<ActivityEvmDto>> GetActivityDetailAsync(int projectId, DateTime dataDate, CancellationToken ct = default)
    {
        var activities = await _activityRepository.GetByProjectIdAsync(projectId, ct);
        var assignments = await _resourceAssignmentRepository.GetByProjectAsync(projectId, ct);
        var directCosts = await _directCostRepository.GetByProjectIdAsync(projectId, ct);
        var actualCosts = await _actualCostRepository.GetByProjectIdAsync(projectId, ct);

        var actualByActivity = actualCosts
            .GroupBy(a => a.ActivityId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var result = new List<ActivityEvmDto>();

        foreach (var activity in activities)
        {
            var resCost = assignments
                .Where(a => a.ActivityId == activity.Id)
                .Sum(a => a.TotalCost);

            var dcCost = directCosts
                .Where(d => d.ActivityId == activity.Id)
                .Sum(d => d.TotalAmount);

            var plannedValue = resCost + dcCost;
            var percentComplete = activity.PercentComplete ?? 0m;
            var earnedValue = plannedValue * (percentComplete / 100m);
            var actualCost = actualByActivity.TryGetValue(activity.Id, out var ac) ? ac : 0m;

            if (plannedValue == 0) continue;

            decimal? cpiVal = actualCost > 0 ? earnedValue / actualCost : null;
            decimal? spiVal = plannedValue > 0 ? earnedValue / plannedValue : null;

            result.Add(new ActivityEvmDto(
                activity.Id,
                activity.Code,
                activity.Name,
                plannedValue,
                earnedValue,
                actualCost,
                percentComplete,
                cpiVal,
                spiVal));
        }

        return result;
    }
}
