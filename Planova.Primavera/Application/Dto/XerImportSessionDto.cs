using System.Text.Json;

namespace Planova.Primavera.Application.Dto;

public class XerImportSessionDto
{
    public Guid Id { get; set; }
    public string SourceFileName { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RowCounts { get; set; }
    public string? ValidationSummary { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public string? TableNames { get; set; }

    public List<string> TableNamesList => string.IsNullOrEmpty(TableNames)
        ? new()
        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(TableNames) ?? new();

    public Dictionary<string, int> RowCountsDict => string.IsNullOrEmpty(RowCounts)
        ? new()
        : JsonSerializer.Deserialize<Dictionary<string, int>>(RowCounts) ?? new();
}
