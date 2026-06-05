using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Application.Dto;

public record CrewDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ProjectId { get; init; }
    public CrewStatus Status { get; init; }
    public string? Category { get; init; }
    public decimal BlendedRate { get; init; }
    public int ResourceCount { get; init; }
    public List<CrewResourceDto> Resources { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CrewResourceDto
{
    public Guid Id { get; init; }
    public Guid CrewId { get; init; }
    public Guid ResourceId { get; init; }
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public decimal Quantity { get; init; }
    public bool IsLead { get; init; }
    public int SortOrder { get; init; }
    public decimal EffectiveRate { get; init; }
    public decimal LineTotal { get; init; }
}

public record UpdateCrewRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ProjectId { get; init; }
    public string? Category { get; init; }
    public List<CrewResourceInput> Resources { get; init; } = [];
}

public record CreateCrewRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ProjectId { get; init; }
    public string? Category { get; init; }
    public List<CrewResourceInput> Resources { get; init; } = [];
}

public record CrewResourceInput
{
    public Guid ResourceId { get; init; }
    public decimal Quantity { get; init; }
    public bool IsLead { get; init; }
}
