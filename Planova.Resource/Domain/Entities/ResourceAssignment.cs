namespace Planova.Resource.Domain.Entities;

public class ResourceAssignment
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid ActivityId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid? CrewId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public string Currency { get; set; } = "USD";
    public string UnitOfMeasure { get; set; } = "hr";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? DurationDays { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Resource Resource { get; set; } = null!;
    public Crew? Crew { get; set; }
    public ICollection<ResourceUsage> UsageRecords { get; set; } = new List<ResourceUsage>();
}
