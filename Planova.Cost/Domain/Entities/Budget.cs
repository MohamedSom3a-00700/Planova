using Planova.Cost.Domain.Enums;

namespace Planova.Cost.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public decimal ResourceCostTotal { get; set; }
    public decimal DirectCostTotal { get; set; }
    public decimal? ContingencyAmount { get; set; }
    public decimal? ContingencyPercent { get; set; }
    public decimal TotalBudget { get; set; }
    public bool IsManualOverride { get; set; }
    public decimal? ManualTotalBudget { get; set; }
    public string Currency { get; set; } = string.Empty;
    public BudgetStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<BudgetRevision> Revisions { get; set; } = new List<BudgetRevision>();
}
