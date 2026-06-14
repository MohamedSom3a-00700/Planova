namespace Planova.Primavera.Application.Dto;

public class PrimaveraCodeDto
{
    public Guid Id { get; set; }
    public string CodeType { get; set; } = string.Empty;
    public string CodeTypeId { get; set; } = string.Empty;
    public string CodeValue { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;
    public string? ParentCodeId { get; set; }
    public string SourceType { get; set; } = string.Empty;
}
