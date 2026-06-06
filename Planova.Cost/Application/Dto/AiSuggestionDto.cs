namespace Planova.Cost.Application.Dto;

public record AiSuggestionDto(
    decimal? SuggestedBudget,
    decimal Confidence,
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
    string Severity
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
