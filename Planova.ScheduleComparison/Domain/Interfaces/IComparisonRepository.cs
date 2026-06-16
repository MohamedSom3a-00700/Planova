using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.ScheduleComparison.Domain.Interfaces;

public interface IComparisonRepository
{
    Task<ComparisonSession?> GetSessionByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ComparisonSession>> GetSessionsByProjectAsync(int projectId, CancellationToken ct = default);
    Task<List<ComparisonResult>> GetResultsBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<ComparisonResult>> GetResultsBySessionIdPagedAsync(Guid sessionId, int skip, int take, CancellationToken ct = default);
    Task AddSessionAsync(ComparisonSession session, CancellationToken ct = default);
    Task UpdateSessionAsync(ComparisonSession session, CancellationToken ct = default);
    Task AddResultsAsync(List<ComparisonResult> results, CancellationToken ct = default);
    Task<ScheduleSnapshot?> GetSnapshotByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ScheduleSnapshot>> GetSnapshotsByProjectAsync(int projectId, CancellationToken ct = default);
    Task AddSnapshotAsync(ScheduleSnapshot snapshot, CancellationToken ct = default);
    Task DeleteSnapshotAsync(ScheduleSnapshot snapshot, CancellationToken ct = default);
    Task<ComparisonRule?> GetRuleByProjectAsync(int projectId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
