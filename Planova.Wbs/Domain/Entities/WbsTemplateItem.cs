using Planova.Wbs.Domain.Enums;

namespace Planova.Wbs.Domain.Entities;

public class WbsTemplateItem
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public WbsLevelType WbsLevel { get; set; }
    public int? DefaultDurationDays { get; set; }
    public decimal? TypicalWeight { get; set; }

    public WbsTemplate Template { get; set; } = null!;
    public WbsTemplateItem? Parent { get; set; }
    public ICollection<WbsTemplateItem> Children { get; set; } = new List<WbsTemplateItem>();
}
