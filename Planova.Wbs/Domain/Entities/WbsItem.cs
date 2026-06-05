using Planova.Wbs.Domain.Enums;

namespace Planova.Wbs.Domain.Entities;

public class WbsItem
{
    public Guid Id { get; set; }
    public Guid WbsId { get; set; }
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public WbsLevelType WbsLevel { get; set; }
    public Guid? SourceBoqItemId { get; set; }
    public decimal? Weight { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedFinish { get; set; }
    public int? DurationDays { get; set; }
    public string? AssignedTo { get; set; }
    public string? Deliverable { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Wbs Wbs { get; set; } = null!;
    public WbsItem? Parent { get; set; }
    public ICollection<WbsItem> Children { get; set; } = new List<WbsItem>();
}
