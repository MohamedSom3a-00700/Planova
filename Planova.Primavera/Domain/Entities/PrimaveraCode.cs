using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraCode
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string CodeType { get; set; } = string.Empty;
    public string CodeTypeId { get; set; } = string.Empty;
    public string CodeValue { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;
    public string? ParentCodeId { get; set; }
    public Guid ImportSessionId { get; set; }
    public PrimaveraSourceType SourceType { get; set; }
}
