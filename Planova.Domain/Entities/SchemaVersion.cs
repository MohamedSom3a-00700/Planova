namespace Planova.Domain.Entities;

public class SchemaVersion
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string? Description { get; set; }
}
