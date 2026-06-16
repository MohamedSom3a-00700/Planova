namespace Planova.ScheduleComparison.Application.Dto;

public record LogicDiffDto
{
    public string PredecessorMatchKey { get; init; } = string.Empty;
    public string SuccessorMatchKey { get; init; } = string.Empty;
    public string ChangeType { get; init; } = string.Empty;
    public string? OldRelationshipType { get; init; }
    public string? NewRelationshipType { get; init; }
    public double? OldLag { get; init; }
    public double? NewLag { get; init; }
}
