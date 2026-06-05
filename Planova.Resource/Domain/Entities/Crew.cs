using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Domain.Entities;

public class Crew
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
    public CrewStatus Status { get; set; } = CrewStatus.Draft;
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CrewResource> Resources { get; set; } = new List<CrewResource>();
}
