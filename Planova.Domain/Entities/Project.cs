namespace Planova.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public int? ClientId { get; set; }
    public int? ContractorId { get; set; }
    public int? SubcontractorId { get; set; }
    public string? Currency { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client? Client { get; set; }
    public Contractor? Contractor { get; set; }
    public Subcontractor? Subcontractor { get; set; }
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
