namespace Planova.ScheduleComparison.Application.Models;

public class ComparisonSourceInfo
{
    public string SourceKind { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public Guid? SnapshotId { get; set; }
    public int? PrimaveraProjectId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime? CapturedAt { get; set; }
}
