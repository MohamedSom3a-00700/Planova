namespace Planova.Primavera.Application.Dto;

public class PrimaveraActivityDto
{
    public Guid Id { get; set; }
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
    public string SourceType { get; set; } = string.Empty;
    public string? UdfValues { get; set; }
}
