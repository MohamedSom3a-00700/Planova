using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Domain.Entities;

public class ProjectParty
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public PartyRole Role { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
