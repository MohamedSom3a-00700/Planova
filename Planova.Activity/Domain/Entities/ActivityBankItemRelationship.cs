using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Entities;

public class ActivityBankItemRelationship
{
    public Guid Id { get; set; }
    public Guid BankId { get; set; }
    public Guid PredecessorItemId { get; set; }
    public Guid SuccessorItemId { get; set; }
    public RelationshipType Type { get; set; }
    public int DefaultLagDays { get; set; }

    public ActivityBank Bank { get; set; } = null!;
    public ActivityBankItem PredecessorItem { get; set; } = null!;
    public ActivityBankItem SuccessorItem { get; set; } = null!;
}
