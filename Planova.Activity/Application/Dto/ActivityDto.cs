namespace Planova.Activity.Application.Dto;

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
