namespace Planova.Cost.Application.Dto;

public record BudgetDto(
    Guid Id,
    int ProjectId,
    decimal ResourceCostTotal,
    decimal DirectCostTotal,
    decimal? ContingencyAmount,
    decimal? ContingencyPercent,
    decimal TotalBudget,
    bool IsManualOverride,
    decimal? ManualTotalBudget,
    string Currency,
    string Status,
    DateTime UpdatedAt,
    string? UpdatedBy,
    List<BudgetRevisionDto> Revisions
);

public record UpdateBudgetRequest(
    decimal? ContingencyAmount,
    decimal? ContingencyPercent,
    bool? IsManualOverride,
    decimal? ManualTotalBudget
);
