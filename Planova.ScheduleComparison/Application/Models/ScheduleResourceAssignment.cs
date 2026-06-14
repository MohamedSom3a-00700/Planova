namespace Planova.ScheduleComparison.Application.Models;

public class ScheduleResourceAssignment
{
    public string ActivityMatchKey { get; set; } = string.Empty;
    public string ActivityProvenanceId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public double? Units { get; set; }
    public decimal? Cost { get; set; }
    public string? AssignmentCode { get; set; }
}
