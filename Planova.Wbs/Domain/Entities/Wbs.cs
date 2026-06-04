using Planova.Wbs.Domain.Enums;

namespace Planova.Wbs.Domain.Entities;

public class Wbs
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Revision { get; set; }
    public WbsStatus Status { get; set; } = WbsStatus.Draft;
    public WbsSource Source { get; set; } = WbsSource.Manual;
    public Guid? SourceBoqId { get; set; }
    public decimal TotalWeight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int ItemCount { get; set; }

    public ICollection<WbsItem> Items { get; set; } = new List<WbsItem>();
}
