using Planova.Cost.Domain.Enums;

namespace Planova.Cost.Domain.Entities;

public class BudgetRevision
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public int RevisionNumber { get; set; }
    public BudgetRevisionType RevisionType { get; set; }
    public decimal Amount { get; set; }
    public BudgetRevisionStatus Status { get; set; }
    public string? Reason { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public Budget Budget { get; set; } = null!;
}
