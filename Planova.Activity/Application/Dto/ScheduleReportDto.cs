namespace Planova.Activity.Application.Dto;

public record ScheduleReportDto
{
    public string ProjectName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<ScheduleReportRowDto> Rows { get; init; } = [];
}

public record ScheduleReportRowDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? Duration { get; init; }
    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedFinish { get; init; }
    public decimal? PercentComplete { get; init; }
    public string Predecessors { get; init; } = string.Empty;
    public string Successors { get; init; } = string.Empty;
}
