using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ReportInstance
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public ReportType ReportType { get; set; }
    public Guid? TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? GeneratedBy { get; set; }
    public string DataSnapshotJson { get; set; } = "{}";
    public string? AiNarrative { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ReportSection> Sections { get; set; } = new();
    public List<ReportExport> Exports { get; set; } = new();
    public ReportTemplate? Template { get; set; }
}
