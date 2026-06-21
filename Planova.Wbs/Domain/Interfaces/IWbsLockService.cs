namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsLockService
{
    Task<LockResult> AcquireLockAsync(Guid wbsId, int userId, CancellationToken ct);
    Task ReleaseLockAsync(Guid wbsId, int userId, CancellationToken ct);
    Task<LockInfo?> GetLockInfoAsync(Guid wbsId, CancellationToken ct);
    Task RefreshLockAsync(Guid wbsId, int userId, CancellationToken ct);
}

public record LockResult(bool Acquired, LockInfo? CurrentLock, string? DeniedReason);

public record LockInfo(Guid WbsId, string LockedByUserName, DateTime LockedAt, DateTime ExpiresAt);
