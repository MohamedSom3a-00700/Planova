namespace Planova.Reporting.Application.Dto;

public record ReportDataDto(
    string ReportType,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string DataJson
);
