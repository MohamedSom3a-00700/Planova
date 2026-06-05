using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Application.Dto;

public record AiSuggestionDto
{
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public decimal SuggestedQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal ConfidenceScore { get; init; }
    public string? Reasoning { get; init; }
}

public record AcceptedSuggestionDto
{
    public string ResourceCode { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
}
