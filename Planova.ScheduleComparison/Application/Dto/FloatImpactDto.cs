namespace Planova.ScheduleComparison.Application.Dto;

public record FloatImpactDto
{
    public string MatchKey { get; init; } = string.Empty;
    public double? OldTotalFloat { get; init; }
    public double? NewTotalFloat { get; init; }
    public double? FloatDelta { get; init; }
    public double? OldFreeFloat { get; init; }
    public double? NewFreeFloat { get; init; }
    public double? FreeFloatDelta { get; init; }
}
