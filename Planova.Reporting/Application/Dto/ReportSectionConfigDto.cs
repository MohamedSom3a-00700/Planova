namespace Planova.Reporting.Application.Dto;

public record ReportSectionConfigDto(
    string SectionId,
    string Title,
    bool IsEnabled,
    int OrderIndex
);
