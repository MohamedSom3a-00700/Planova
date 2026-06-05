namespace Planova.Wbs.Domain.Entities;

public class WbsTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public bool IsStandard { get; set; }
    public int Version { get; set; }
    public string Tags { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WbsTemplateItem> Items { get; set; } = new List<WbsTemplateItem>();
}
