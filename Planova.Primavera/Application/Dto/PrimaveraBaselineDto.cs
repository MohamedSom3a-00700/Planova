namespace Planova.Primavera.Application.Dto;

public class PrimaveraBaselineDto
{
    public Guid Id { get; set; }
    public string BaselineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SourceType { get; set; } = string.Empty;
}
