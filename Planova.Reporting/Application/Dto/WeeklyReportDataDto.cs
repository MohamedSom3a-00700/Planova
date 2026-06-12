namespace Planova.Reporting.Application.Dto;

public record WeeklyProgressByWbs(
    string WbsCode,
    string WbsName,
    int TotalActivities,
    int CompletedActivities,
    decimal PercentComplete,
    decimal? Weight
);

public record WeeklyResourceUsage(
    string ResourceName,
    string ResourceType,
    decimal TotalQuantity,
    string UnitOfMeasure
);

public record WeeklyDelayItem(
    string ActivityName,
    string ActivityCode,
    DateTime? PlannedFinish,
    DateTime? ActualFinish,
    int DelayDays
);

public record WeeklyLookAheadItem(
    string ActivityName,
    string ActivityCode,
    DateTime? PlannedStart,
    DateTime? PlannedFinish,
    string Status
);

public record WeeklyReportDataDto(
    DateTime WeekStart,
    DateTime WeekEnd,
    List<WeeklyProgressByWbs> ProgressByWbs,
    List<WeeklyResourceUsage> ResourceUsage,
    List<WeeklyDelayItem> Delays,
    List<WeeklyLookAheadItem> LookAhead
);
