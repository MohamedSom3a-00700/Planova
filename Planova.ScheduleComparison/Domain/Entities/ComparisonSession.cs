using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Domain.Entities;

public class ComparisonSession
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public ComparisonMode Mode { get; set; }
    public SessionState State { get; set; }
    public string SourceKind { get; set; } = string.Empty;
    public Guid? SourceSnapshotId { get; set; }
    public int? SourcePrimaveraProjectId { get; set; }
    public string SourceLabel { get; set; } = string.Empty;
    public DateTime? SourceCapturedAt { get; set; }
    public string TargetKind { get; set; } = string.Empty;
    public Guid? TargetSnapshotId { get; set; }
    public int? TargetPrimaveraProjectId { get; set; }
    public string TargetLabel { get; set; } = string.Empty;
    public DateTime? TargetCapturedAt { get; set; }
    public string IncludedScopes { get; set; } = string.Empty;
    public string? ResultJson { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<ComparisonResult> Results { get; set; } = new();
}
