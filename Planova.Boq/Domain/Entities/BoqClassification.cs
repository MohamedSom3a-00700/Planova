using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Domain.Entities;

public class BoqClassification
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ClassificationScope Scope { get; set; }
    public Guid? ProjectId { get; set; }

    public BoqClassification? Parent { get; set; }
    public ICollection<BoqClassification> Children { get; set; } = new List<BoqClassification>();
}
