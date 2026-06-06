# Application DTOs — Cost Studio

## Budget

```csharp
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
```

## BudgetRevision

```csharp
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
```

## DirectCost

```csharp
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
```

## CostBreakdown

```csharp
public record CostBreakdownDto(
    string NodeType,         // "Project", "Wbs", "Activity", "ResourceCost", "DirectCost"
    string? NodeId,          // Entity ID (null for computed nodes)
    string? ParentNodeId,
    string Label,
    decimal PlannedCost,
    decimal ActualCost,
    decimal Variance,
    List<CostBreakdownDto> Children
);
```

## CostBaseline

```csharp
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
```

## ActualCost

```csharp
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
```

## CashFlow

```csharp
public record CashFlowPeriodDto(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal PlannedCost,
    decimal ActualCost,
    decimal CumulativePlanned,
    decimal CumulativeActual
);

public enum CashFlowPeriodType
{
    Weekly,
    Monthly
}
```

## EVM

```csharp
public record EvmMetricsDto(
    DateTime DataDate,
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost,
    decimal CostVariance,
    decimal ScheduleVariance,
    decimal? CostPerformanceIndex,   // null when AC=0
    decimal? SchedulePerformanceIndex, // null when PV=0
    decimal EstimateAtCompletion,
    decimal EstimateToComplete,
    decimal VarianceAtCompletion,
    decimal BudgetAtCompletion,
    string StatusColor               // "Green", "Amber", "Red"
);

public record ActivityEvmDto(
    Guid ActivityId,
    string ActivityCode,
    string ActivityName,
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost,
    decimal PercentComplete,
    decimal? Cpi,
    decimal? Spi
);
```

## AI Services

```csharp
public record AiSuggestionDto(
    decimal? SuggestedBudget,
    decimal Confidence,              // 0.0 to 1.0
    string? Reasoning,
    bool IsAvailable,
    string? UnavailableMessage
);

public record CostAnomalyDto(
    Guid ActivityId,
    string ActivityCode,
    string ActivityName,
    decimal PlannedCost,
    decimal ActualCost,
    decimal VariancePercent,
    string Severity                  // "Low", "Medium", "High", "Critical"
);

public record AiForecastDto(
    decimal FormulaBasedEac,
    decimal AiAdjustedEac,
    decimal Confidence,
    string? Reasoning,
    bool IsAvailable,
    string? UnavailableMessage
);

public record AiNarrativeDto(
    string Narrative,
    bool IsAvailable,
    string? UnavailableMessage
);
```

## Report

```csharp
public enum CostReportType
{
    CostBreakdown,
    CashFlow,
    Evm,
    BudgetSummary
}

public record ReportResultDto(
    CostReportType ReportType,
    int ProjectId,
    DateTime GeneratedAt,
    // Type-specific content varies by report type
    object? Data,
    string? AiNarrative
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
```
