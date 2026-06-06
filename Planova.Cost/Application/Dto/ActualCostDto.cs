namespace Planova.Cost.Application.Dto;

public record ActualCostDto(
    Guid Id,
    int ProjectId,
    Guid ActivityId,
    decimal Amount,
    string Currency,
    string Source,
    string? ImportBatchId,
    DateTime EntryDate,
    bool IsOrphaned,
    DateTime CreatedAt
);

public record SetActualCostRequest(
    Guid ActivityId,
    decimal Amount,
    string Currency
);

public record ActivityVarianceDto(
    Guid ActivityId,
    string ActivityCode,
    string ActivityName,
    decimal PlannedCost,
    decimal ActualCost,
    decimal Variance,
    decimal VariancePercent
);

public record ImportResultDto(
    int TotalRows,
    int MatchedRows,
    int UnmatchedRows,
    int UpdatedRows,
    int CreatedRows,
    bool Success,
    List<string> Errors,
    List<string> Warnings
);
