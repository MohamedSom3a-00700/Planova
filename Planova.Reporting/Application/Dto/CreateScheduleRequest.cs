namespace Planova.Reporting.Application.Dto;

public record CreateScheduleRequest(
    int ProjectId,
    string ReportType,
    string Frequency,
    int? DayOfWeek,
    int? DayOfMonth,
    string TimeOfDay,
    string TimeZoneId,
    string ExportFormats,
    int MaxRetries
);
