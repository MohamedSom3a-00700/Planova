namespace Planova.Cost.Application.Dto;

public record CostBreakdownDto(
    string NodeType,
    string? NodeId,
    string? ParentNodeId,
    string Label,
    decimal PlannedCost,
    decimal ActualCost,
    decimal Variance,
    List<CostBreakdownDto> Children
);
