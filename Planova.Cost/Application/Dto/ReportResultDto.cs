namespace Planova.Cost.Application.Dto;

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
    object? Data,
    string? AiNarrative
);
