using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Application.Dto;

public record ResourceAssignmentDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public Guid ActivityId { get; init; }
    public string ActivityName { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public Guid? CrewId { get; init; }
    public string? CrewName { get; init; }
    public decimal Quantity { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal TotalCost { get; init; }
    public decimal? DurationDays { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateAssignmentRequest
{
    public int ProjectId { get; init; }
    public Guid ActivityId { get; init; }
    public Guid ResourceId { get; init; }
    public Guid? CrewId { get; init; }
    public decimal Quantity { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = "USD";
    public string UnitOfMeasure { get; init; } = "hr";
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
}

public record UpdateAssignmentRequest
{
    public Guid Id { get; init; }
    public decimal Quantity { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = "USD";
    public string UnitOfMeasure { get; init; } = "hr";
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
}
