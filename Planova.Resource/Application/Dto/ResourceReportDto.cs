namespace Planova.Resource.Application.Dto;

public record ResourceUsageReportDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<ActivityResourceSection> Activities { get; init; } = [];
    public decimal TotalCost { get; init; }
}

public record ActivityResourceSection
{
    public string ActivityCode { get; init; } = string.Empty;
    public string ActivityName { get; init; } = string.Empty;
    public List<ResourceAssignmentDto> Assignments { get; init; } = [];
    public decimal ActivityTotalCost { get; init; }
}

public record ResourceCostReportDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<CostSummarySection> CostSections { get; init; } = [];
    public decimal GrandTotal { get; init; }
}

public record CostSummarySection
{
    public string SectionName { get; init; } = string.Empty;
    public List<CostLineItem> LineItems { get; init; } = [];
}

public record CostLineItem
{
    public string Label { get; init; } = string.Empty;
    public decimal TotalCost { get; init; }
    public string Currency { get; init; } = "USD";
    public int AssignmentCount { get; init; }
}

public enum ReportType
{
    UsageSummary,
    CostReport
}
