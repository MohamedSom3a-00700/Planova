namespace Planova.Primavera.Domain.Entities;

public class XerExportProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IncludeRawTables { get; set; }
    public string? SelectedEntityTypes { get; set; }
    public string? OutputPathTemplate { get; set; }
    public DateTime CreatedAt { get; set; }
}
