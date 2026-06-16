using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Domain.Entities;

public class ComparisonResult
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string MatchKey { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public MatchConfidence MatchConfidence { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Severity { get; set; } = "Info";

    public ComparisonSession? Session { get; set; }
}
