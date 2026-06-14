namespace Planova.Primavera.Application.Dto;

public class PrimaveraRelationshipDto
{
    public Guid Id { get; set; }
    public string PredTaskId { get; set; } = string.Empty;
    public string SuccTaskId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double LagDuration { get; set; }
    public string SourceType { get; set; } = string.Empty;
}
