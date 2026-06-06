using Planova.Cost.Application.Dto;
using Planova.Cost.Application.Mappings;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Interfaces;
using Planova.Activity.Domain.Interfaces;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Cost.Application.Services;

public class CostService : ICostService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IDirectCostRepository _directCostRepository;
    private readonly ICostBaselineRepository _baselineRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IResourceAssignmentRepository _resourceAssignmentRepository;
    private readonly IActualCostRepository _actualCostRepository;

    public CostService(
        IBudgetRepository budgetRepository,
        IDirectCostRepository directCostRepository,
        ICostBaselineRepository baselineRepository,
        IActivityRepository activityRepository,
        IResourceAssignmentRepository resourceAssignmentRepository,
        IActualCostRepository actualCostRepository)
    {
        _budgetRepository = budgetRepository;
        _directCostRepository = directCostRepository;
        _baselineRepository = baselineRepository;
        _activityRepository = activityRepository;
        _resourceAssignmentRepository = resourceAssignmentRepository;
        _actualCostRepository = actualCostRepository;
    }

    public async Task<CostBreakdownDto> GetCostBreakdownAsync(int projectId, CancellationToken ct = default)
    {
        var activities = await _activityRepository.GetByProjectIdAsync(projectId, ct);
        var directCosts = await _directCostRepository.GetByProjectIdAsync(projectId, ct);
        var actualCosts = await _actualCostRepository.GetByProjectIdAsync(projectId, ct);

        var directCostsByActivity = directCosts
            .Where(d => d.ActivityId.HasValue)
            .GroupBy(d => d.ActivityId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var actualCostsByActivity = actualCosts
            .GroupBy(a => a.ActivityId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var activityNodes = new List<CostBreakdownDto>();

        foreach (var activity in activities)
        {
            var activityId = activity.Id;
            var resCost = await _resourceAssignmentRepository.GetTotalCostForActivityAsync(activityId, ct);

            var dcList = directCostsByActivity.TryGetValue(activityId, out var dc) ? dc : new List<DirectCost>();
            var dcTotal = dcList.Sum(d => d.TotalAmount);

            var acTotal = actualCostsByActivity.TryGetValue(activityId, out var ac) ? ac : 0m;

            var children = new List<CostBreakdownDto>();

            if (resCost > 0)
            {
                children.Add(new CostBreakdownDto("ResourceCost", activityId.ToString(), null, "Resource Assignments", resCost, 0, resCost, new List<CostBreakdownDto>()));
            }

            foreach (var dcItem in dcList)
            {
                children.Add(new CostBreakdownDto("DirectCost", dcItem.Id.ToString(), activityId.ToString(), dcItem.Description ?? dcItem.Category.ToString(), dcItem.TotalAmount, 0, dcItem.TotalAmount, new List<CostBreakdownDto>()));
            }

            if (acTotal > 0)
            {
                children.Add(new CostBreakdownDto("ActualCost", activityId.ToString(), null, "Actual Costs", 0, acTotal, -acTotal, new List<CostBreakdownDto>()));
            }

            var planned = resCost + dcTotal;
            activityNodes.Add(new CostBreakdownDto(
                "Activity", activityId.ToString(), projectId.ToString(),
                $"[{activity.Code}] {activity.Name}",
                planned, acTotal, planned - acTotal,
                children));
        }

        var totalPlanned = activityNodes.Sum(c => c.PlannedCost);
        var totalActual = activityNodes.Sum(c => c.ActualCost);

        return new CostBreakdownDto(
            "Project", projectId.ToString(), null, $"Project {projectId}",
            totalPlanned, totalActual, totalPlanned - totalActual,
            activityNodes);
    }

    public async Task<BudgetDto> GetBudgetAsync(int projectId, CancellationToken ct = default)
    {
        var budget = await _budgetRepository.GetByProjectIdAsync(projectId, ct);
        if (budget == null)
            throw new InvalidOperationException($"No budget found for project {projectId}");

        return budget.ToDto();
    }

    public async Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken ct = default)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId, ct);
        if (budget == null)
            throw new InvalidOperationException($"Budget {budgetId} not found");

        if (request.ContingencyAmount.HasValue)
            budget.ContingencyAmount = request.ContingencyAmount.Value;
        if (request.ContingencyPercent.HasValue)
            budget.ContingencyPercent = request.ContingencyPercent.Value;
        if (request.IsManualOverride.HasValue)
            budget.IsManualOverride = request.IsManualOverride.Value;
        if (request.ManualTotalBudget.HasValue)
            budget.ManualTotalBudget = request.ManualTotalBudget.Value;

        budget.UpdatedAt = DateTime.UtcNow;

        if (budget.IsManualOverride && budget.ManualTotalBudget.HasValue)
            budget.TotalBudget = budget.ManualTotalBudget.Value;
        else
            budget.TotalBudget = budget.ResourceCostTotal + budget.DirectCostTotal + (budget.ContingencyAmount ?? 0);

        await _budgetRepository.UpdateAsync(budget, ct);
        return budget.ToDto();
    }

    public async Task<bool> HasResourceCostsChangedAsync(int projectId, CancellationToken ct = default)
    {
        var budget = await _budgetRepository.GetByProjectIdAsync(projectId, ct);
        if (budget == null) return false;

        return false;
    }

    public async Task<CostBaselineDto> SetBaselineAsync(int projectId, CreateBaselineRequest request, CancellationToken ct = default)
    {
        var active = await _baselineRepository.GetActiveBaselineAsync(projectId, ct);
        if (active != null)
        {
            active.IsActive = false;
            await _baselineRepository.UpdateAsync(active, ct);
        }

        var activities = await _activityRepository.GetByProjectIdAsync(projectId, ct);
        var rows = new List<CostBaselineRow>();

        foreach (var activity in activities)
        {
            var resCost = await _resourceAssignmentRepository.GetTotalCostForActivityAsync(activity.Id, ct);
            var dcList = await _directCostRepository.GetByActivityIdAsync(activity.Id, ct);
            var dcTotal = dcList.Sum(d => d.TotalAmount);
            var plannedCost = resCost + dcTotal;

            if (plannedCost > 0 || activity.PlannedStart.HasValue)
            {
                rows.Add(new CostBaselineRow
                {
                    Id = Guid.NewGuid(),
                    ActivityId = activity.Id,
                    PlannedCost = plannedCost,
                    PlannedStart = activity.PlannedStart ?? DateTime.UtcNow,
                    PlannedFinish = activity.PlannedFinish ?? DateTime.UtcNow.AddMonths(1),
                    BudgetAtCompletion = plannedCost
                });
            }
        }

        var totalPlanned = rows.Sum(r => r.PlannedCost);

        var baseline = new CostBaseline
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            Rows = rows.Count > 0 ? rows : new List<CostBaselineRow>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ActivityId = Guid.Empty,
                    PlannedCost = totalPlanned,
                    PlannedStart = DateTime.UtcNow,
                    PlannedFinish = DateTime.UtcNow.AddMonths(6),
                    BudgetAtCompletion = totalPlanned
                }
            }
        };

        await _baselineRepository.AddAsync(baseline, ct);
        return baseline.ToDto();
    }
}
