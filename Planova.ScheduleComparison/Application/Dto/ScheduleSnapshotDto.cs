namespace Planova.ScheduleComparison.Application.Dto;

public record ScheduleSnapshotDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public string Label { get; init; } = string.Empty;
    public Guid? BaselineId { get; init; }
    public DateTime CapturedAt { get; init; }
    public int ActivityCount { get; init; }
    public int RelationshipCount { get; init; }
}
