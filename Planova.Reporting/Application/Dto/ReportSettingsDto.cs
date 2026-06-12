namespace Planova.Reporting.Application.Dto;

public record ReportSettingsDto(
    Guid Id,
    int ProjectId,
    string ReportType,
    string EnabledSectionsJson,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
