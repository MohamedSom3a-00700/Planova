namespace Planova.Activity.Application.Dto;

public record ActivityRelationshipDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public Guid PredecessorId { get; init; }
    public string PredecessorCode { get; init; } = string.Empty;
    public string PredecessorName { get; init; } = string.Empty;
    public Guid SuccessorId { get; init; }
    public string SuccessorCode { get; init; } = string.Empty;
    public string SuccessorName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int LagDays { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateRelationshipRequest
{
    public int ProjectId { get; init; }
    public Guid PredecessorId { get; init; }
    public Guid SuccessorId { get; init; }
    public string Type { get; init; } = "FS";
    public int LagDays { get; init; }
    public string? Description { get; init; }
}

public record UpdateRelationshipRequest
{
    public Guid Id { get; init; }
    public string? Type { get; init; }
    public int? LagDays { get; init; }
    public string? Description { get; init; }
}

public record CircularReferenceCheckResult
{
    public bool HasCycle { get; init; }
    public List<Guid> CycleActivities { get; init; } = [];
    public string? Message { get; init; }
}
