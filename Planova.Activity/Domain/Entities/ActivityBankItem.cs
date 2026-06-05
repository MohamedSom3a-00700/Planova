using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Entities;

public class ActivityBankItem
{
    public Guid Id { get; set; }
    public Guid BankId { get; set; }
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public int DefaultDuration { get; set; }
    public ActivityType DefaultActivityType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ActivityBank Bank { get; set; } = null!;
    public ActivityBankItem? Parent { get; set; }
    public ICollection<ActivityBankItem> Children { get; set; } = new List<ActivityBankItem>();
}
