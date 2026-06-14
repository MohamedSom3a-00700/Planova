namespace Planova.Primavera.Application.Dto;

public class PrimaveraProjectDto
{
    public Guid Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public bool IsActive { get; set; }
}
