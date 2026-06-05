namespace Planova.Resource.Domain.Entities;

public class ResourceRate
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal Rate { get; set; }
    public string Currency { get; set; } = "USD";
    public string UnitOfMeasure { get; set; } = "hr";
    public bool IsDefault { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resource Resource { get; set; } = null!;
}
