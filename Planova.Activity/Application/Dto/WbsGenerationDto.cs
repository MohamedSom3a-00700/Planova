namespace Planova.Activity.Application.Dto;

public record WbsGenerationRequest
{
    public int ProjectId { get; init; }
    public string Mode { get; init; } = "Simple";
    public List<Guid> WbsItemIds { get; init; } = [];
    public Guid? BankId { get; init; }
    public bool ReplaceExisting { get; init; }
    public bool MergeExisting { get; init; }
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
    public bool IsNew { get; init; }
}
