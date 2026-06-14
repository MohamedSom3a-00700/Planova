namespace Planova.ScheduleComparison.Domain.Entities;

public class ScheduleSnapshot
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string Label { get; set; } = string.Empty;
    public Guid? BaselineId { get; set; }
    public string SnapshotData { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public int ActivityCount { get; set; }
    public int RelationshipCount { get; set; }
}
