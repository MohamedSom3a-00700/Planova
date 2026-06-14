namespace Planova.ScheduleComparison.Application.Models;

public class ScheduleActivity
{
    public string ProvenanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string? WbsCode { get; set; }
    public string? ActivityCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Status { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? Finish { get; set; }
    public double? Duration { get; set; }
    public double? OriginalDuration { get; set; }
    public double? RemainingDuration { get; set; }
    public double? PercentComplete { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualFinish { get; set; }
    public DateTime? EarlyStart { get; set; }
    public DateTime? EarlyFinish { get; set; }
    public DateTime? LateStart { get; set; }
    public DateTime? LateFinish { get; set; }
    public double? TotalFloat { get; set; }
    public double? FreeFloat { get; set; }
    public double? RemainingFloat { get; set; }
    public bool IsCritical { get; set; }
    public string? Calendar { get; set; }
    public string? CalendarId { get; set; }
    public string? WbsPath { get; set; }
    public Dictionary<string, string> Codes { get; set; } = new();
    public Dictionary<string, string> UdfValues { get; set; } = new();
}
