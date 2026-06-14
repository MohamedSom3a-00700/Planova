using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraRelationship
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string PredTaskId { get; set; } = string.Empty;
    public string SuccTaskId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double LagDuration { get; set; }
    public Guid? BaselineId { get; set; }
    public Guid ImportSessionId { get; set; }
    public PrimaveraSourceType SourceType { get; set; }
}
