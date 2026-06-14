using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraRepairAction
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid IssueId { get; set; }
    public string Description { get; set; } = string.Empty;
    public PrimaveraEntityType TargetEntityType { get; set; }
    public string? TargetEntityIds { get; set; }
    public string AppliedBy { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public bool UndoAvailable { get; set; }
    public PrimaveraSourceType SourceType { get; set; }
}
