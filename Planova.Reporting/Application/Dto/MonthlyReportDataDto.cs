namespace Planova.Reporting.Application.Dto;

public record MonthlyEvmSummary(
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost,
    decimal CostVariance,
    decimal ScheduleVariance,
    decimal? CostPerformanceIndex,
    decimal? SchedulePerformanceIndex,
    decimal EstimateAtCompletion,
    decimal BudgetAtCompletion,
    decimal VarianceAtCompletion,
    string StatusColor
);

public record MonthlyBudgetVsActual(
    string Category,
    decimal Budget,
    decimal Actual,
    decimal Variance
);

public record MonthlyProgressByWbs(
    string WbsCode,
    string WbsName,
    decimal PercentComplete,
    decimal? Weight
);

public record MonthlyResourceProductivity(
    string ResourceName,
    string ResourceType,
    decimal TotalQuantity,
    decimal TotalCost,
    string UnitOfMeasure
);

public record MonthlyReportDataDto(
    DateTime MonthStart,
    DateTime MonthEnd,
    MonthlyEvmSummary? EvmSummary,
    List<MonthlyBudgetVsActual> BudgetVsActual,
    List<MonthlyProgressByWbs> ProgressByWbs,
    List<MonthlyResourceProductivity> ResourceProductivity
);
