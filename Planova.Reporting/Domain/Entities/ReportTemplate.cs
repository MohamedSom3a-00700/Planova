using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ReportTemplate
{
    public Guid Id { get; set; }
    public int? ProjectId { get; set; }
    public ReportType ReportType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public string LayoutJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
