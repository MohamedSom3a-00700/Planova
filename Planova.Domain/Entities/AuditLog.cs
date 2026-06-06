namespace Planova.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public int? ProjectId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? PreviousState { get; set; }
    public string? NewState { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
