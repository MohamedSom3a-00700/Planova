namespace Planova.Reporting.Application.Dto;

public record ReportInstanceDto(
    Guid Id,
    int ProjectId,
    string ReportType,
    Guid? TemplateId,
    string Title,
    string Status,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime GeneratedAt,
    string? GeneratedBy,
    string DataSnapshotJson,
    string? AiNarrative,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public bool ExportExists { get; set; }
}
