namespace Planova.Domain.Entities;

public class ProjectPartyLink
{
    public int ProjectId { get; set; }
    public Guid PartyId { get; set; }
    public string Role { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public Party Party { get; set; } = null!;
}
