namespace Planova.Cost.Application.Dto;

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
