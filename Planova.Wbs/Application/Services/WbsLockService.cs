using System.Collections.Concurrent;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

public sealed class WbsLockService : IWbsLockService
{
    private static readonly ConcurrentDictionary<Guid, LockEntry> _locks = new();
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(30);

    public Task<LockResult> AcquireLockAsync(Guid wbsId, int userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var entry = _locks.GetOrAdd(wbsId, _ => new LockEntry
        {
            UserId = userId,
            UserName = $"User {userId}",
            LockedAt = now,
            ExpiresAt = now.Add(LockTimeout)
        });

        if (entry.ExpiresAt <= now)
        {
            entry.UserId = userId;
            entry.UserName = $"User {userId}";
            entry.LockedAt = now;
            entry.ExpiresAt = now.Add(LockTimeout);

            return Task.FromResult(new LockResult(
                true,
                new LockInfo(wbsId, entry.UserName, entry.LockedAt, entry.ExpiresAt),
                null));
        }

        if (entry.UserId != userId)
        {
            return Task.FromResult(new LockResult(
                false,
                new LockInfo(wbsId, entry.UserName, entry.LockedAt, entry.ExpiresAt),
                $"Locked by {entry.UserName} until {entry.ExpiresAt:HH:mm} UTC"));
        }

        entry.ExpiresAt = now.Add(LockTimeout);
        return Task.FromResult(new LockResult(
            true,
            new LockInfo(wbsId, entry.UserName, entry.LockedAt, entry.ExpiresAt),
            null));
    }

    public Task ReleaseLockAsync(Guid wbsId, int userId, CancellationToken ct)
    {
        if (_locks.TryGetValue(wbsId, out var entry) && entry.UserId == userId)
        {
            _locks.TryRemove(wbsId, out _);
        }
        return Task.CompletedTask;
    }

    public Task<LockInfo?> GetLockInfoAsync(Guid wbsId, CancellationToken ct)
    {
        if (_locks.TryGetValue(wbsId, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
        {
            return Task.FromResult<LockInfo?>(new LockInfo(wbsId, entry.UserName, entry.LockedAt, entry.ExpiresAt));
        }
        return Task.FromResult<LockInfo?>(null);
    }

    public Task RefreshLockAsync(Guid wbsId, int userId, CancellationToken ct)
    {
        if (_locks.TryGetValue(wbsId, out var entry) && entry.UserId == userId)
        {
            entry.ExpiresAt = DateTime.UtcNow.Add(LockTimeout);
        }
        return Task.CompletedTask;
    }

    private sealed class LockEntry
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime LockedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
