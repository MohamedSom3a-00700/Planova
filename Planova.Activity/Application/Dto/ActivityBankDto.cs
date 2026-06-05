namespace Planova.Activity.Application.Dto;

public record ActivityBankDto
{
    public Guid Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Subcategory { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsStandard { get; init; }
    public int Version { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<ActivityBankItemDto> Items { get; init; } = [];
    public List<ActivityBankItemRelationshipDto> Relationships { get; init; } = [];
}

public record ActivityBankItemDto
{
    public Guid Id { get; init; }
    public Guid BankId { get; init; }
    public Guid? ParentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public int SortOrder { get; init; }
    public int DefaultDuration { get; init; }
    public string DefaultActivityType { get; init; } = "Task";
}

public record ActivityBankItemRelationshipDto
{
    public Guid Id { get; init; }
    public Guid BankId { get; init; }
    public Guid PredecessorItemId { get; init; }
    public Guid SuccessorItemId { get; init; }
    public string Type { get; init; } = "FS";
    public int DefaultLagDays { get; init; }
}

public record CreateBankEntryRequest
{
    public string Category { get; init; } = string.Empty;
    public string? Subcategory { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<ActivityBankItemDto> Items { get; init; } = [];
    public List<ActivityBankItemRelationshipDto> Relationships { get; init; } = [];
}

public record UpdateBankEntryRequest
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<string>? Tags { get; init; }
    public List<ActivityBankItemDto>? Items { get; init; }
    public List<ActivityBankItemRelationshipDto>? Relationships { get; init; }
}
