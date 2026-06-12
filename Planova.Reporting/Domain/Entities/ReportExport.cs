using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ReportExport
{
    public Guid Id { get; set; }
    public Guid ReportInstanceId { get; set; }
    public ExportFormat Format { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime ExportedAt { get; set; }
    public string? ExportedBy { get; set; }

    public ReportInstance ReportInstance { get; set; } = null!;
}
