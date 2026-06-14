namespace Planova.Primavera.Application.Dto;

public class PrimaveraResourceAssignmentDto
{
    public Guid Id { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public double Units { get; set; }
    public double PlannedUnits { get; set; }
    public double ActualUnits { get; set; }
    public decimal CostPerUnit { get; set; }
    public string SourceType { get; set; } = string.Empty;
}
