using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Entities;

public class ActivityRelationship
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid PredecessorId { get; set; }
    public Guid SuccessorId { get; set; }
    public RelationshipType Type { get; set; }
    public int LagDays { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Activity Predecessor { get; set; } = null!;
    public Activity Successor { get; set; } = null!;
}
