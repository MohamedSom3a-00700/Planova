namespace Planova.Excel.Models;

public class MappingProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int Version { get; set; }
    public Dictionary<string, string> ColumnMappings { get; set; } = new();
    public List<string> ValidationRules { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
