using Planova.Activity.Domain.Entities;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityBankItemRepository
{
    Task<List<ActivityBankItem>> GetByBankIdAsync(Guid bankId, CancellationToken ct = default);
    Task<ActivityBankItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ActivityBankItem> items, CancellationToken ct = default);
    Task UpdateAsync(ActivityBankItem item, CancellationToken ct = default);
    Task DeleteByBankIdAsync(Guid bankId, CancellationToken ct = default);
}
