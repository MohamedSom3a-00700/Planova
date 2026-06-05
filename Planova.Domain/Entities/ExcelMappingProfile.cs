namespace Planova.Domain.Entities;

public class ExcelMappingProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int Version { get; set; }
    public string ColumnMappingsJson { get; set; } = "{}";
    public string ValidationRulesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
