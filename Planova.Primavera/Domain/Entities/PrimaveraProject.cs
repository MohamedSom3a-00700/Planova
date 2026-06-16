namespace Planova.Primavera.Domain.Entities;

public class PrimaveraProject
{
    public Guid Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public bool IsActive { get; set; }
    public int ImportSessionId { get; set; }
    public DateTime? LastRecalcDate { get; set; }
    public DateTime? PlanStartDate { get; set; }
    public DateTime? PlanEndDate { get; set; }
    public DateTime? SchedEndDate { get; set; }
    public DateTime? AddDate { get; set; }
    public DateTime? LastTasksumDate { get; set; }
    public DateTime? LastScheduleDate { get; set; }
}
