using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class XerImportSession
{
    public Guid Id { get; set; }
    public PrimaveraImportStatus Status { get; set; }
    public string SourceFileName { get; set; } = string.Empty;
    public string SourceFileHash { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public string? RowCounts { get; set; }
    public string? ValidationSummary { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public string? TableNames { get; set; }
}
