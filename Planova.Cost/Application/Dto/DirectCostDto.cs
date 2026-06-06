namespace Planova.Cost.Application.Dto;

public record DirectCostDto(
    Guid Id,
    int ProjectId,
    Guid? ActivityId,
    string Category,
    string? CustomCategoryName,
    string Description,
    decimal Quantity,
    string UnitOfMeasure,
    decimal UnitRate,
    string Currency,
    decimal TotalAmount,
    string Scope,
    bool IsOrphaned,
    Guid? DeletedActivityId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateDirectCostRequest(
    int ProjectId,
    Guid? ActivityId,
    string Category,
    string? CustomCategoryName,
    string Description,
    decimal Quantity,
    string UnitOfMeasure,
    decimal UnitRate,
    string Currency,
    string Scope
);

public record UpdateDirectCostRequest(
    string Description,
    decimal Quantity,
    decimal UnitRate,
    string? CustomCategoryName
);
