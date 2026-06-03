namespace Planova.Excel.Models;

public class ImportRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string WorksheetName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Dictionary<string, string> ColumnMappings { get; set; } = new();
    public Guid? MappingProfileId { get; set; }
    public int BatchSize { get; set; } = 1000;
    public DuplicateStrategy DuplicateHandling { get; set; } = DuplicateStrategy.Prompt;
}
