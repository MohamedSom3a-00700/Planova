namespace Planova.Cost.Domain.Entities;

public class CostBaselineRow
{
    public Guid Id { get; set; }
    public Guid BaselineId { get; set; }
    public Guid ActivityId { get; set; }
    public decimal PlannedCost { get; set; }
    public DateTime PlannedStart { get; set; }
    public DateTime PlannedFinish { get; set; }
    public decimal BudgetAtCompletion { get; set; }

    public CostBaseline Baseline { get; set; } = null!;
}
