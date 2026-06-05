using Planova.Resource.Domain.Entities;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceRateRepository
{
    Task<ResourceRate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceRate>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default);
    Task<ResourceRate?> GetEffectiveRateAsync(Guid resourceId, DateTime date, CancellationToken ct = default);
    Task<bool> HasDuplicateEffectiveDateAsync(Guid resourceId, DateTime effectiveDate, Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(ResourceRate rate, CancellationToken ct = default);
    Task UpdateAsync(ResourceRate rate, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task BulkUpdateAsync(List<(Guid ResourceId, decimal NewRate)> rateUpdates, DateTime effectiveDate, CancellationToken ct = default);
}
