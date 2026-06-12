namespace Planova.Reporting.Application.Dto;

public record ReportTemplateDto(
    Guid Id,
    int? ProjectId,
    string ReportType,
    string Name,
    string? Description,
    bool IsDefault,
    string LayoutJson,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
