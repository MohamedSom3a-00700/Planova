# Application DTOs: Activity Studio

```csharp
namespace Planova.Activity.Application.Dto;

// ─── Activity ───

public record ActivityDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public Guid? ParentActivityId { get; init; }
    public Guid? WbsItemId { get; init; }
    public string? WbsItemCode { get; init; }
    public string? WbsItemName { get; init; }
    public Guid? CalendarId { get; init; }
    public string? CalendarName { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ActivityType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? Duration { get; init; }
    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedFinish { get; init; }
    public DateTime? ActualStart { get; init; }
    public DateTime? ActualFinish { get; init; }
    public decimal? PercentComplete { get; init; }
    public decimal? Weight { get; init; }
    public string? Notes { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateActivityRequest
{
    public int ProjectId { get; init; }
    public Guid? ParentActivityId { get; init; }
    public Guid? WbsItemId { get; init; }
    public Guid? CalendarId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ActivityType { get; init; } = "Task";
    public int? Duration { get; init; }
    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedFinish { get; init; }
    public decimal? Weight { get; init; }
    public string? Notes { get; init; }
}

public record UpdateActivityRequest
{
    public Guid Id { get; init; }
    public Guid? ParentActivityId { get; init; }
    public Guid? WbsItemId { get; init; }
    public Guid? CalendarId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public int? Duration { get; init; }
    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedFinish { get; init; }
    public DateTime? ActualStart { get; init; }
    public DateTime? ActualFinish { get; init; }
    public decimal? PercentComplete { get; init; }
    public decimal? Weight { get; init; }
    public string? Notes { get; init; }
}

public record ActivityFilter
{
    public string? Status { get; init; }
    public string? ActivityType { get; init; }
    public Guid? WbsItemId { get; init; }
    public string? SearchText { get; init; }
}

// ─── ActivityRelationship ───

public record ActivityRelationshipDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public Guid PredecessorId { get; init; }
    public string PredecessorCode { get; init; } = string.Empty;
    public string PredecessorName { get; init; } = string.Empty;
    public Guid SuccessorId { get; init; }
    public string SuccessorCode { get; init; } = string.Empty;
    public string SuccessorName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int LagDays { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateRelationshipRequest
{
    public int ProjectId { get; init; }
    public Guid PredecessorId { get; init; }
    public Guid SuccessorId { get; init; }
    public string Type { get; init; } = "FS";
    public int LagDays { get; init; }
    public string? Description { get; init; }
}

public record UpdateRelationshipRequest
{
    public Guid Id { get; init; }
    public string? Type { get; init; }
    public int? LagDays { get; init; }
    public string? Description { get; init; }
}

public record CircularReferenceCheckResult
{
    public bool HasCycle { get; init; }
    public List<Guid> CycleActivities { get; init; } = [];
    public string? Message { get; init; }
}

// ─── Calendar ───

public record CalendarDto
{
    public Guid Id { get; init; }
    public int? ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal HoursPerDay { get; init; }
    public int DaysPerWeek { get; init; }
    public bool Monday { get; init; }
    public bool Tuesday { get; init; }
    public bool Wednesday { get; init; }
    public bool Thursday { get; init; }
    public bool Friday { get; init; }
    public bool Saturday { get; init; }
    public bool Sunday { get; init; }
    public bool IsDefault { get; init; }
    public string? Description { get; init; }
}

public record CreateCalendarRequest
{
    public int? ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal HoursPerDay { get; init; } = 8;
    public int DaysPerWeek { get; init; } = 5;
    public bool Monday { get; init; } = true;
    public bool Tuesday { get; init; } = true;
    public bool Wednesday { get; init; } = true;
    public bool Thursday { get; init; } = true;
    public bool Friday { get; init; }
    public bool Saturday { get; init; }
    public bool Sunday { get; init; }
    public bool IsDefault { get; init; }
    public string? Description { get; init; }
}

public record UpdateCalendarRequest
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public decimal? HoursPerDay { get; init; }
    public int? DaysPerWeek { get; init; }
    public bool? Monday { get; init; }
    public bool? Tuesday { get; init; }
    public bool? Wednesday { get; init; }
    public bool? Thursday { get; init; }
    public bool? Friday { get; init; }
    public bool? Saturday { get; init; }
    public bool? Sunday { get; init; }
    public bool? IsDefault { get; init; }
    public string? Description { get; init; }
}

public record CalendarDayDto
{
    public Guid Id { get; init; }
    public Guid CalendarId { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Label { get; init; }
}

// ─── ActivityBank ───

public record ActivityBankDto
{
    public Guid Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Subcategory { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsStandard { get; init; }
    public int Version { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<ActivityBankItemDto> Items { get; init; } = [];
    public List<ActivityBankItemRelationshipDto> Relationships { get; init; } = [];
}

public record ActivityBankItemDto
{
    public Guid Id { get; init; }
    public Guid BankId { get; init; }
    public Guid? ParentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public int SortOrder { get; init; }
    public int DefaultDuration { get; init; }
    public string DefaultActivityType { get; init; } = "Task";
}

public record ActivityBankItemRelationshipDto
{
    public Guid Id { get; init; }
    public Guid BankId { get; init; }
    public Guid PredecessorItemId { get; init; }
    public Guid SuccessorItemId { get; init; }
    public string Type { get; init; } = "FS";
    public int DefaultLagDays { get; init; }
}

public record CreateBankEntryRequest
{
    public string Category { get; init; } = string.Empty;
    public string? Subcategory { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<ActivityBankItemDto> Items { get; init; } = [];
    public List<ActivityBankItemRelationshipDto> Relationships { get; init; } = [];
}

public record UpdateBankEntryRequest
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<string>? Tags { get; init; }
    public List<ActivityBankItemDto>? Items { get; init; }
    public List<ActivityBankItemRelationshipDto>? Relationships { get; init; }
}

// ─── WBS Generation ───

public record WbsGenerationRequest
{
    public int ProjectId { get; init; }
    public string Mode { get; init; } = "Simple"; // "Simple" or "Bank"
    public List<Guid> WbsItemIds { get; init; } = [];
    public Guid? BankId { get; init; }
    public bool ReplaceExisting { get; init; } // false = merge (append)
    public Guid? CalendarId { get; init; }
}

public record WbsGenerationPreviewDto
{
    public List<Guid> WbsItemIds { get; init; } = [];
    public int TotalActivities { get; init; }
    public List<ActivityPreviewItem> Previews { get; init; } = [];
    public bool HasExistingActivities { get; init; }
    public int ExistingActivityCount { get; init; }
}

public record ActivityPreviewItem
{
    public Guid WbsItemId { get; init; }
    public string WbsItemName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "Task";
    public int Duration { get; init; }
    public bool IsNew { get; init; } // false if already exists and merging
}

// ─── Reports ───

public record ScheduleReportDto
{
    public string ProjectName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<ScheduleReportRowDto> Rows { get; init; } = [];
}

public record ScheduleReportRowDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? Duration { get; init; }
    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedFinish { get; init; }
    public decimal? PercentComplete { get; init; }
    public string Predecessors { get; init; } = string.Empty;
    public string Successors { get; init; } = string.Empty;
}
```
