namespace Planova.ScheduleComparison.Application.Models;

public class ScheduleComparisonResult
{
    public string SchemaVersion { get; set; } = "1.0";
    public Guid SessionId { get; set; }
    public int ProjectId { get; set; }
    public string Mode { get; set; } = string.Empty;
    public DateTime ComparedAt { get; set; }
    public ComparisonSourceInfo Source { get; set; } = new();
    public ComparisonSourceInfo Target { get; set; } = new();
    public List<string> IncludedScopes { get; set; } = new();
    public string GeneratedByVersion { get; set; } = string.Empty;
    public List<ActivityDiff> ActivityDiffs { get; set; } = new();
    public List<LogicDiff> LogicDiffs { get; set; } = new();
    public List<ResourceDiff> ResourceDiffs { get; set; } = new();
    public CriticalPathDiff? CriticalPathDiffResult { get; set; }
    public FloatImpactReport? FloatReport { get; set; }
    public ComparisonSummary Summary { get; set; } = new();
}

public class ActivityDiff
{
    public string MatchKey { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Severity { get; set; } = "Info";
}

public class LogicDiff
{
    public string PredecessorMatchKey { get; set; } = string.Empty;
    public string SuccessorMatchKey { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string? OldRelationshipType { get; set; }
    public string? NewRelationshipType { get; set; }
    public double? OldLag { get; set; }
    public double? NewLag { get; set; }
}

public class ResourceDiff
{
    public string ActivityMatchKey { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public double? OldUnits { get; set; }
    public double? NewUnits { get; set; }
    public decimal? OldCost { get; set; }
    public decimal? NewCost { get; set; }
}

public class CriticalPathDiff
{
    public double? SourceDuration { get; set; }
    public double? TargetDuration { get; set; }
    public double? DurationChange { get; set; }
    public List<string> EnteredCriticalPath { get; set; } = new();
    public List<string> ExitedCriticalPath { get; set; } = new();
    public List<string> RemainedOnCriticalPath { get; set; } = new();
}

public class FloatImpactReport
{
    public List<ActivityFloatDelta> ActivityFloatDeltas { get; set; } = new();
    public List<string> ActivitiesWithNegativeFloat { get; set; } = new();
    public List<string> ActivitiesWithImprovedFloat { get; set; } = new();
    public List<string> ActivitiesWithWorsenedFloat { get; set; } = new();
}

public class ActivityFloatDelta
{
    public string MatchKey { get; set; } = string.Empty;
    public double? OldTotalFloat { get; set; }
    public double? NewTotalFloat { get; set; }
    public double? FloatDelta { get; set; }
    public double? OldFreeFloat { get; set; }
    public double? NewFreeFloat { get; set; }
    public double? FreeFloatDelta { get; set; }
}

public class ComparisonSummary
{
    public int TotalActivities { get; set; }
    public int AddedActivities { get; set; }
    public int RemovedActivities { get; set; }
    public int ModifiedActivities { get; set; }
    public int TotalRelationships { get; set; }
    public int AddedRelationships { get; set; }
    public int RemovedRelationships { get; set; }
    public int ModifiedRelationships { get; set; }
    public int TotalResourceAssignments { get; set; }
    public int AddedAssignments { get; set; }
    public int RemovedAssignments { get; set; }
    public int ModifiedAssignments { get; set; }
    public double? CriticalPathDurationDelta { get; set; }
    public int ActivitiesWithFloatLoss { get; set; }
    public int ActivitiesWithFloatGain { get; set; }
}
