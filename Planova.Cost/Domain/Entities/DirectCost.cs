using Planova.Cost.Domain.Enums;

namespace Planova.Cost.Domain.Entities;

public class DirectCost
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid? ActivityId { get; set; }
    public DirectCostCategory Category { get; set; }
    public string? CustomCategoryName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DirectCostScope Scope { get; set; }
    public bool IsOrphaned { get; set; }
    public Guid? DeletedActivityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
