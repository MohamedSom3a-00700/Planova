using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Entities;

namespace Planova.Cost.Application.Mappings;

public static class CostMappingProfile
{
    public static BudgetDto ToDto(this Budget entity) => new(
        entity.Id,
        entity.ProjectId,
        entity.ResourceCostTotal,
        entity.DirectCostTotal,
        entity.ContingencyAmount,
        entity.ContingencyPercent,
        entity.TotalBudget,
        entity.IsManualOverride,
        entity.ManualTotalBudget,
        entity.Currency,
        entity.Status.ToString(),
        entity.UpdatedAt,
        entity.UpdatedBy,
        entity.Revisions.Select(r => r.ToDto()).ToList()
    );

    public static BudgetRevisionDto ToDto(this BudgetRevision entity) => new(
        entity.Id,
        entity.BudgetId,
        entity.RevisionNumber,
        entity.RevisionType.ToString(),
        entity.Amount,
        entity.Status.ToString(),
        entity.Reason,
        entity.ApprovedBy,
        entity.ApprovedAt,
        entity.CreatedAt,
        entity.CreatedBy ?? string.Empty
    );

    public static DirectCostDto ToDto(this DirectCost entity) => new(
        entity.Id,
        entity.ProjectId,
        entity.ActivityId,
        entity.Category.ToString(),
        entity.CustomCategoryName,
        entity.Description,
        entity.Quantity,
        entity.UnitOfMeasure,
        entity.UnitRate,
        entity.Currency,
        entity.TotalAmount,
        entity.Scope.ToString(),
        entity.IsOrphaned,
        entity.DeletedActivityId,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    public static CostBaselineDto ToDto(this CostBaseline entity) => new(
        entity.Id,
        entity.ProjectId,
        entity.Description,
        entity.IsActive,
        entity.CreatedAt,
        entity.CreatedBy
    );

    public static CostBaselineRowDto ToDto(this CostBaselineRow entity) => new(
        entity.ActivityId,
        entity.PlannedCost,
        entity.PlannedStart,
        entity.PlannedFinish,
        entity.BudgetAtCompletion
    );

    public static ActualCostDto ToDto(this ActualCost entity) => new(
        entity.Id,
        entity.ProjectId,
        entity.ActivityId,
        entity.Amount,
        entity.Currency,
        entity.Source.ToString(),
        entity.ImportBatchId,
        entity.EntryDate,
        entity.IsOrphaned,
        entity.CreatedAt
    );
}
