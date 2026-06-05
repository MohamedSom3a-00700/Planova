using Planova.Activity.Domain.Entities;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityBankRepository
{
    Task<ActivityBank?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityBank>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<List<ActivityBank>> SearchAsync(string query, CancellationToken ct = default);
    Task AddAsync(ActivityBank bank, CancellationToken ct = default);
    Task UpdateAsync(ActivityBank bank, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsStandardAsync(Guid id, CancellationToken ct = default);
}
