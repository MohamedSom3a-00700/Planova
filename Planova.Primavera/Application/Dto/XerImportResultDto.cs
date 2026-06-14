namespace Planova.Primavera.Application.Dto;

public class XerImportResultDto
{
    public bool Success { get; set; }
    public Guid? ImportSessionId { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, int> RowCounts { get; set; } = new();
}
