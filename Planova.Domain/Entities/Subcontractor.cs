namespace Planova.Domain.Entities;

public class Subcontractor
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? OrganizationDetails { get; set; }
    public string? Trade { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Logo { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}