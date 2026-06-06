namespace Planova.Cost.Application.Dto;

public record BudgetRevisionDto(
    Guid Id,
    Guid BudgetId,
    int RevisionNumber,
    string RevisionType,
    decimal Amount,
    string Status,
    string? Reason,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    string CreatedBy
);

public record CreateBudgetRevisionRequest(
    string RevisionType,
    decimal Amount,
    string? Reason
);
