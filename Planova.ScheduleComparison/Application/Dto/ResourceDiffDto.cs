namespace Planova.ScheduleComparison.Application.Dto;

public record ResourceDiffDto
{
    public string ActivityMatchKey { get; init; } = string.Empty;
    public string ResourceId { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public string ChangeType { get; init; } = string.Empty;
    public double? OldUnits { get; init; }
    public double? NewUnits { get; init; }
    public decimal? OldCost { get; init; }
    public decimal? NewCost { get; init; }
}
