using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Domain.Interfaces;

public interface IScheduleComparisonService
{
    Task<ComparisonSession> CompareAsync(
        int projectId,
        Guid? sourceSnapshotId,
        Guid? targetSnapshotId,
        string sourceKind,
        string targetKind,
        string sourceLabel,
        string targetLabel,
        List<ComparisonScope> scopes,
        Guid? ruleId = null,
        CancellationToken ct = default);

    Task<ComparisonSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<ComparisonSession>> ListSessionsAsync(int projectId, CancellationToken ct = default);
    Task ReOpenSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task SoftDeleteSessionAsync(Guid sessionId, CancellationToken ct = default);
}
