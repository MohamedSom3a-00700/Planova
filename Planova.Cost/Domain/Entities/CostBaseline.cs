namespace Planova.Cost.Domain.Entities;

public class CostBaseline
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<CostBaselineRow> Rows { get; set; } = new List<CostBaselineRow>();
}
