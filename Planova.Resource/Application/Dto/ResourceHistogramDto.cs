using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Application.Dto;

public record ResourceHistogramDto
{
    public List<HistogramDayDto> DailyData { get; init; } = [];
    public List<HistogramResourceSummary> ResourceSummaries { get; init; } = [];
    public DateTime ProjectStart { get; init; }
    public DateTime ProjectEnd { get; init; }
    public int TotalDays { get; init; }
    public HistogramFilter AppliedFilter { get; init; } = null!;
}

public record HistogramDayDto
{
    public DateTime Date { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal? AvailableQuantity { get; init; }
    public bool IsOverallocated { get; init; }
    public ResourceType? ResourceType { get; init; }
    public List<HistogramBreakdownItem> Breakdown { get; init; } = [];
}

public record HistogramBreakdownItem
{
    public Guid ResourceId { get; init; }
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal? AvailableQuantity { get; init; }
}

public record HistogramResourceSummary
{
    public Guid ResourceId { get; init; }
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public decimal TotalQuantity { get; init; }
}

public record HistogramFilter
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public ResourceType? ResourceType { get; init; }
    public Guid? ResourceId { get; init; }
    public HistogramAggregation Aggregation { get; init; } = HistogramAggregation.Sum;
}
