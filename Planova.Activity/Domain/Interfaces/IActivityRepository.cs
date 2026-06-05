using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Interfaces;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

public interface IActivityRepository
{
    Task<ActivityEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityEntity>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<ActivityEntity>> GetByWbsItemIdAsync(Guid wbsItemId, CancellationToken ct = default);
    Task<List<ActivityEntity>> GetByStatusAsync(int projectId, ActivityStatus status, CancellationToken ct = default);
    Task<List<ActivityEntity>> GetChildrenAsync(Guid parentActivityId, CancellationToken ct = default);
    Task<string> GetNextCodeAsync(int projectId, CancellationToken ct = default);
    Task AddAsync(ActivityEntity activity, CancellationToken ct = default);
    Task UpdateAsync(ActivityEntity activity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
