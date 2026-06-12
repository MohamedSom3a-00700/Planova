using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ReportSettings
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public ReportType ReportType { get; set; }
    public string EnabledSectionsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
