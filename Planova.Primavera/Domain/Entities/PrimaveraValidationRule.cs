using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraValidationRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PrimaveraValidationSeverity Severity { get; set; }
    public bool IsEnabled { get; set; }
    public string Category { get; set; } = string.Empty;
}
