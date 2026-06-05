namespace Planova.Resource.Domain.Entities;

public class ResourceUsage
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime Date { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }

    public ResourceAssignment Assignment { get; set; } = null!;
    public Resource Resource { get; set; } = null!;
}
