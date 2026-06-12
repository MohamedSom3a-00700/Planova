namespace Planova.Reporting.Application.Dto;

public record ReportSectionDto(
    Guid Id,
    Guid ReportInstanceId,
    string SectionType,
    string Title,
    int OrderIndex,
    string ContentJson,
    DateTime CreatedAt
);
