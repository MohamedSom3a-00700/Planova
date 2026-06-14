namespace Planova.ScheduleComparison.Application.Dto;

public record ComparisonSummaryDto
{
    public int TotalChanges { get; init; }
    public int Added { get; init; }
    public int Removed { get; init; }
    public int Modified { get; init; }
    public int Unchanged { get; init; }
    public int ActivityChanges { get; init; }
    public int LogicChanges { get; init; }
    public int ResourceChanges { get; init; }
    public bool HasCriticalPathChanges { get; init; }
    public bool HasFloatChanges { get; init; }
}
