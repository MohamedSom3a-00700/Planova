namespace Planova.Boq.Domain.Entities;

public class Boq
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = string.Empty;
    public BoqStatus Status { get; set; } = BoqStatus.Draft;
    public int RevisionNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ImportSource { get; set; }
    public DateTime? ImportedAt { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;

    public ICollection<BoqItem> Items { get; set; } = new List<BoqItem>();
}
