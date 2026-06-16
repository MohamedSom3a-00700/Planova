using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.ScheduleComparison.Domain.Interfaces;

public interface IScheduleSnapshotService
{
    Task<ScheduleSnapshot> CaptureSnapshotAsync(int projectId, string label, CancellationToken ct = default);
    Task<ScheduleSnapshot?> RestoreSnapshotAsync(Guid snapshotId, CancellationToken ct = default);
    Task<List<ScheduleSnapshot>> ListSnapshotsAsync(int projectId, CancellationToken ct = default);
    Task DeleteSnapshotAsync(Guid snapshotId, CancellationToken ct = default);
}
