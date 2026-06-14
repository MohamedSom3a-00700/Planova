namespace Planova.ScheduleComparison.Application.Dto;

public record ComparisonSessionDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public string Mode { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string SourceLabel { get; init; } = string.Empty;
    public string TargetLabel { get; init; } = string.Empty;
    public string? Error { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
