namespace Planova.Reporting.Application.Dto;

public record ReportExportDto(
    Guid Id,
    Guid ReportInstanceId,
    string Format,
    string FilePath,
    long FileSizeBytes,
    DateTime ExportedAt,
    string? ExportedBy
);
