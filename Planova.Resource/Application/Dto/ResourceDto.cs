using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Application.Dto;

public record ResourceDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public ResourceScope Scope { get; init; }
    public int? ProjectId { get; init; }
    public ResourceStatus Status { get; init; }
    public decimal DefaultRate { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? MaxQuantity { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Trade { get; init; }
    public string? SkillLevel { get; init; }
    public string? EquipmentType { get; init; }
    public string? Capacity { get; init; }
    public decimal? OperatingCost { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? WastagePercent { get; init; }
    public string? Company { get; init; }
    public decimal? ContractValue { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
    public decimal? EffectiveRate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateResourceRequest
{
    public string Name { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public ResourceScope Scope { get; init; }
    public int? ProjectId { get; init; }
    public decimal DefaultRate { get; init; }
    public string UnitOfMeasure { get; init; } = "hr";
    public decimal? MaxQuantity { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Trade { get; init; }
    public string? SkillLevel { get; init; }
    public string? EquipmentType { get; init; }
    public string? Capacity { get; init; }
    public decimal? OperatingCost { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? WastagePercent { get; init; }
    public string? Company { get; init; }
    public decimal? ContractValue { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
}

public record UpdateResourceRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal DefaultRate { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? MaxQuantity { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Trade { get; init; }
    public string? SkillLevel { get; init; }
    public string? EquipmentType { get; init; }
    public string? Capacity { get; init; }
    public decimal? OperatingCost { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? WastagePercent { get; init; }
    public string? Company { get; init; }
    public decimal? ContractValue { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
}

public record ResourceFilter
{
    public string? SearchQuery { get; init; }
    public ResourceType? Type { get; init; }
    public ResourceScope? Scope { get; init; }
    public int? ProjectId { get; init; }
    public ResourceStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record ResourceDuplicateCheckResult
{
    public bool HasDuplicate { get; init; }
    public ResourceDto? MatchingResource { get; init; }
    public string? WarningMessage { get; init; }
}
