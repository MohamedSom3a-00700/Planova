using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Domain.Entities;

public class BoqImportSession
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int TotalSheetsDetected { get; set; }
    public int TotalSheetsImported { get; set; }
    public int TotalSectionsCreated { get; set; }
    public int TotalItemsImported { get; set; }
    public decimal TotalAmount { get; set; }
    public ImportMode ImportMode { get; set; }
    public DateTime ImportedAt { get; set; }
    public int? ImportedByUserId { get; set; }
    public ImportStatus Status { get; set; }

    public ICollection<BoqWorksheetMapping> WorksheetMappings { get; set; } = new List<BoqWorksheetMapping>();
}
