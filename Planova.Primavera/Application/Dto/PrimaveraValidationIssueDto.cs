namespace Planova.Primavera.Application.Dto;

public class PrimaveraValidationIssueDto
{
    public Guid Id { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
    public bool IsResolved { get; set; }
}
