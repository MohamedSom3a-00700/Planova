using Planova.Cost.Domain.Enums;

namespace Planova.Cost.Domain.Entities;

public class ActualCost
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid ActivityId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ActualCostSource Source { get; set; }
    public string? ImportBatchId { get; set; }
    public DateTime EntryDate { get; set; }
    public bool IsOrphaned { get; set; }
    public Guid? DeletedActivityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
