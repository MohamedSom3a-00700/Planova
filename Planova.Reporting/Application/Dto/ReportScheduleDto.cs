namespace Planova.Reporting.Application.Dto;

public record ReportScheduleDto(
    Guid Id,
    int ProjectId,
    string ReportType,
    Guid? TemplateId,
    string Frequency,
    int? DayOfWeek,
    int? DayOfMonth,
    string TimeOfDay,
    string TimeZoneId,
    string ExportFormats,
    bool IsActive,
    DateTime? LastRunAt,
    string? LastStatus,
    string? LastErrorMessage,
    DateTime? LastSuccessfulRunAt,
    int RetryCount,
    int MaxRetries,
    DateTime NextRunAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
