using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ReportSection
{
    public Guid Id { get; set; }
    public Guid ReportInstanceId { get; set; }
    public ReportSectionType SectionType { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string ContentJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }

    public ReportInstance ReportInstance { get; set; } = null!;
}
