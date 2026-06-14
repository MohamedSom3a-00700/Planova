using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraActivity
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string? WbsId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double Duration { get; set; }
    public double RemainingDuration { get; set; }
    public double PercentComplete { get; set; }
    public string? CalendarId { get; set; }
    public Guid? BaselineId { get; set; }
    public int? BaselineVersion { get; set; }
    public Guid ImportSessionId { get; set; }
    public PrimaveraSourceType SourceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UdfValues { get; set; }
}
