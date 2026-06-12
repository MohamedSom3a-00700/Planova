namespace Planova.Reporting.Application.Dto;

public record DailyProgressItem(
    string ActivityName,
    string ActivityCode,
    decimal? PercentComplete,
    string Status,
    DateTime? PlannedStart,
    DateTime? PlannedFinish,
    DateTime? ActualStart,
    DateTime? ActualFinish
);

public record DailyWorkforceItem(
    string ResourceName,
    string ResourceCode,
    string ActivityName,
    decimal Quantity,
    string UnitOfMeasure
);

public record DailyEquipmentItem(
    string EquipmentName,
    string EquipmentCode,
    string ActivityName,
    decimal Quantity,
    string UnitOfMeasure
);

public record DailyIssueItem(
    string Title,
    string ActivityName,
    string Status,
    string? Priority,
    DateTime ReportedAt
);

public record DailyPhotoItem(
    string FileName,
    string? Notes,
    DateTime UploadedAt
);

public record DailyReportDataDto(
    DateTime ReportDate,
    int TotalActivities,
    int CompletedActivities,
    int InProgressActivities,
    int NotStartedActivities,
    decimal OverallPercentComplete,
    List<DailyProgressItem> ProgressToday,
    List<DailyWorkforceItem> Workforce,
    List<DailyEquipmentItem> Equipment,
    List<DailyIssueItem> Issues,
    List<DailyPhotoItem> Photos
);
