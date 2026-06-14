namespace Planova.Primavera.Application.Dto;

public class PrimaveraRepairActionDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TargetEntityType { get; set; } = string.Empty;
    public string AppliedBy { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public bool UndoAvailable { get; set; }
}
