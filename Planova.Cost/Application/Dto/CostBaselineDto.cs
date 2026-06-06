namespace Planova.Cost.Application.Dto;

public record CostBaselineDto(
    Guid Id,
    int ProjectId,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedBy
);

public record CreateBaselineRequest(
    int ProjectId,
    string? Description
);

public record CostBaselineRowDto(
    Guid ActivityId,
    decimal PlannedCost,
    DateTime PlannedStart,
    DateTime PlannedFinish,
    decimal BudgetAtCompletion
);
