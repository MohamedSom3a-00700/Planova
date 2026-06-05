namespace Planova.Activity.Domain.Entities;

public class ActivityBank
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsStandard { get; set; }
    public int Version { get; set; }
    public string Tags { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ActivityBankItem> Items { get; set; } = new List<ActivityBankItem>();
    public ICollection<ActivityBankItemRelationship> Relationships { get; set; } = new List<ActivityBankItemRelationship>();
}
