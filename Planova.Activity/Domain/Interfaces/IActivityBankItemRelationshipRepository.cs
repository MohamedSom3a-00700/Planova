using Planova.Activity.Domain.Entities;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityBankItemRelationshipRepository
{
    Task<List<ActivityBankItemRelationship>> GetByBankIdAsync(Guid bankId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ActivityBankItemRelationship> relationships, CancellationToken ct = default);
    Task DeleteByBankIdAsync(Guid bankId, CancellationToken ct = default);
}
