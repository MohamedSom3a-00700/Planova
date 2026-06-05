using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Entities;

public class Activity
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid? ParentActivityId { get; set; }
    public Guid? WbsItemId { get; set; }
    public Guid? CalendarId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityType ActivityType { get; set; }
    public ActivityStatus Status { get; set; }
    public int? Duration { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedFinish { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualFinish { get; set; }
    public decimal? PercentComplete { get; set; }
    public decimal? Weight { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Activity? ParentActivity { get; set; }
    public ICollection<Activity> Children { get; set; } = new List<Activity>();
}
