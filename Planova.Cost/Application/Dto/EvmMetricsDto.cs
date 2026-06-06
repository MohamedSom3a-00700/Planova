namespace Planova.Cost.Application.Dto;

public record EvmMetricsDto(
    DateTime DataDate,
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost,
    decimal CostVariance,
    decimal ScheduleVariance,
    decimal? CostPerformanceIndex,
    decimal? SchedulePerformanceIndex,
    decimal EstimateAtCompletion,
    decimal EstimateToComplete,
    decimal VarianceAtCompletion,
    decimal BudgetAtCompletion,
    string StatusColor
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
