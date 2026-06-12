namespace Planova.Reporting.Application.Dto;

public record ExecutiveKpiCard(
    string Label,
    decimal Value,
    string Unit,
    string? Trend,
    string StatusColor
);

public record ExecutiveSCurvePoint(
    DateTime Date,
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost
);

public record ExecutiveFinancialOverview(
    decimal TotalBudget,
    decimal TotalActualCost,
    decimal TotalVariance,
    decimal EstimateAtCompletion,
    decimal VarianceAtCompletion,
    decimal? CostPerformanceIndex
);

public record ExecutiveMilestoneItem(
    string ActivityName,
    string ActivityCode,
    string Status,
    DateTime? PlannedStart,
    DateTime? PlannedFinish,
    DateTime? ActualStart,
    DateTime? ActualFinish,
    decimal PercentComplete
);

public record ExecutiveProjectParty(
    string Name,
    string? LogoPath,
    string Role
);

public record ExecutiveReportDataDto(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    List<ExecutiveKpiCard> KpiCards,
    List<ExecutiveSCurvePoint> SCurve,
    ExecutiveFinancialOverview? FinancialOverview,
    List<ExecutiveMilestoneItem> Milestones,
    List<ExecutiveProjectParty> ProjectParties
);
