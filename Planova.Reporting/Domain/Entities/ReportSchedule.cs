using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ReportSchedule
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public ReportType ReportType { get; set; }
    public Guid? TemplateId { get; set; }
    public ScheduleFrequency Frequency { get; set; }
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public TimeSpan TimeOfDay { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public string ExportFormats { get; set; } = "[]";
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastStatus { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime? LastSuccessfulRunAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTime NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ReportTemplate? Template { get; set; }
}
