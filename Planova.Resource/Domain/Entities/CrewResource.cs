namespace Planova.Resource.Domain.Entities;

public class CrewResource
{
    public Guid Id { get; set; }
    public Guid CrewId { get; set; }
    public Guid ResourceId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsLead { get; set; }
    public int SortOrder { get; set; }

    public Crew Crew { get; set; } = null!;
    public Resource Resource { get; set; } = null!;
}
