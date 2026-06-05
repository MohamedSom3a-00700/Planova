using Planova.Resource.Domain.Entities;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceAssignmentRepository
{
    Task<ResourceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceAssignment>> GetByActivityAsync(Guid activityId, CancellationToken ct = default);
    Task<List<ResourceAssignment>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<List<ResourceAssignment>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default);
    Task<bool> HasAssignmentsForActivityAsync(Guid activityId, CancellationToken ct = default);
    Task<bool> HasAssignmentsForResourceAsync(Guid resourceId, CancellationToken ct = default);
    Task AddAsync(ResourceAssignment assignment, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ResourceAssignment> assignments, CancellationToken ct = default);
    Task UpdateAsync(ResourceAssignment assignment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<decimal> GetTotalCostForActivityAsync(Guid activityId, CancellationToken ct = default);
}
