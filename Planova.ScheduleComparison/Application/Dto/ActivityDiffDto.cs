namespace Planova.ScheduleComparison.Application.Dto;

public record ActivityDiffDto
{
    public string MatchKey { get; init; } = string.Empty;
    public string ActivityName { get; init; } = string.Empty;
    public string FieldName { get; init; } = string.Empty;
    public string ChangeType { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string Severity { get; init; } = "Info";
}
