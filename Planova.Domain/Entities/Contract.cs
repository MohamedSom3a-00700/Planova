namespace Planova.Domain.Entities;

public class Contract
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public string? Currency { get; set; }
    public DateTime? AwardDate { get; set; }
    public DateTime? CommencementDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Status { get; set; }
    public int ProjectId { get; set; }
    public int ClientId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public Client Client { get; set; } = null!;
}
