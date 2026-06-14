namespace Planova.Primavera.Application.Dto;

public class XerImportPreviewDto
{
    public Guid SessionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Dictionary<string, int> RowCounts { get; set; } = new();
    public List<string> UnsupportedTables { get; set; } = new();
    public List<PrimaveraValidationIssueDto> ValidationIssues { get; set; } = new();
    public bool CanCommit => ValidationIssues.All(v => v.Severity != "Error");
}
