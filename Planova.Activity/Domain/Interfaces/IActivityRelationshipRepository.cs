using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityRelationshipRepository
{
    Task<ActivityRelationship?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityRelationship>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<ActivityRelationship>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task AddAsync(ActivityRelationship relationship, CancellationToken ct = default);
    Task UpdateAsync(ActivityRelationship relationship, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid predecessorId, Guid successorId, RelationshipType type, CancellationToken ct = default);
}
