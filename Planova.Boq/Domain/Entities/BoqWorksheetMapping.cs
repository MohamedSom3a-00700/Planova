namespace Planova.Boq.Domain.Entities;

public class BoqWorksheetMapping
{
    public Guid Id { get; set; }
    public Guid ImportSessionId { get; set; }
    public string WorksheetName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public decimal MatchConfidence { get; set; }
    public string ColumnMappings { get; set; } = string.Empty;
    public int RowsImported { get; set; }
    public decimal SectionAmount { get; set; }

    public BoqImportSession ImportSession { get; set; } = null!;
}
