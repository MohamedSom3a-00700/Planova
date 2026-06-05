using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceUsageRepository
{
    Task<List<ResourceUsage>> GetByResourceAsync(Guid resourceId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<List<ResourceUsage>> GetByProjectAsync(int projectId, DateTime? from = null, DateTime? to = null, ResourceType? typeFilter = null, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ResourceUsage> usages, CancellationToken ct = default);
    Task DeleteByAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
    Task RegenerateForAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
}
