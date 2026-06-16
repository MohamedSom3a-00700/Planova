namespace Planova.ScheduleComparison.Application.Models;

public class ScheduleRelationship
{
    public string PredecessorProvenanceId { get; set; } = string.Empty;
    public string PredecessorActivityId { get; set; } = string.Empty;
    public string PredecessorMatchKey { get; set; } = string.Empty;
    public string SuccessorProvenanceId { get; set; } = string.Empty;
    public string SuccessorActivityId { get; set; } = string.Empty;
    public string SuccessorMatchKey { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = "FS";
    public double? Lag { get; set; }
    public string? LagCalendar { get; set; }
}
