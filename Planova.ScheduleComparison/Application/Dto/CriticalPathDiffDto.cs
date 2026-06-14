namespace Planova.ScheduleComparison.Application.Dto;

public record CriticalPathDiffDto
{
    public double? SourceDuration { get; init; }
    public double? TargetDuration { get; init; }
    public double? DurationChange { get; init; }
    public List<string> EnteredCriticalPath { get; init; } = new();
    public List<string> ExitedCriticalPath { get; init; } = new();
    public List<string> RemainedOnCriticalPath { get; init; } = new();
}
