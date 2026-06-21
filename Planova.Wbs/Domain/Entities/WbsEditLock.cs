namespace Planova.Wbs.Domain.Entities;

public class WbsEditLock
{
    public Guid Id { get; set; }
    public Guid WbsId { get; set; }
    public int LockedByUserId { get; set; }
    public DateTime LockedAt { get; set; }
    public DateTime LockExpiresAt { get; set; }

    public Wbs Wbs { get; set; } = null!;
}
