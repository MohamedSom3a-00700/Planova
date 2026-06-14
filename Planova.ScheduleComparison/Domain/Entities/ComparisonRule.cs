namespace Planova.ScheduleComparison.Domain.Entities;

public class ComparisonRule
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double SeverityThresholdCritical { get; set; }
    public double SeverityThresholdMajor { get; set; }
    public double SeverityThresholdMinor { get; set; }
    public bool EnableFuzzyMatching { get; set; }
    public string MatchingStrategyPreference { get; set; } = string.Empty;
}
