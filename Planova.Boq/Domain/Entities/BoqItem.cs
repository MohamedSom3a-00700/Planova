namespace Planova.Boq.Domain.Entities;

public class BoqItem
{
    public Guid Id { get; set; }
    public Guid BoqId { get; set; }
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public ItemType ItemType { get; set; } = ItemType.Item;
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public Guid? ClassificationId { get; set; }
    public string? CostCode { get; set; }
    public bool IsActive { get; set; } = true;

    public Boq Boq { get; set; } = null!;
    public BoqItem? Parent { get; set; }
    public ICollection<BoqItem> Children { get; set; } = new List<BoqItem>();
}
