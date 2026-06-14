using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraResourceAssignment
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public double Units { get; set; }
    public double PlannedUnits { get; set; }
    public double ActualUnits { get; set; }
    public decimal CostPerUnit { get; set; }
    public Guid? BaselineId { get; set; }
    public Guid ImportSessionId { get; set; }
    public PrimaveraSourceType SourceType { get; set; }
}
