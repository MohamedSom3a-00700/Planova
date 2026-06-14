using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraValidationIssue
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid RuleId { get; set; }
    public PrimaveraValidationSeverity Severity { get; set; }
    public PrimaveraEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
    public bool IsResolved { get; set; }
    public DateTime DetectedAt { get; set; }
    public Guid? ImportSessionId { get; set; }
}
