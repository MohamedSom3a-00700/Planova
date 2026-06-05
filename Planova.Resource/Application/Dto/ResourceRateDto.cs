namespace Planova.Resource.Application.Dto;

public record ResourceRateDto
{
    public Guid Id { get; init; }
    public Guid ResourceId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateRateRequest
{
    public Guid ResourceId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = "USD";
    public string UnitOfMeasure { get; init; } = "hr";
    public bool IsDefault { get; init; }
    public string? Notes { get; init; }
}
