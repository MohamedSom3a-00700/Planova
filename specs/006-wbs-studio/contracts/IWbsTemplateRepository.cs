// IWbsTemplateRepository — Repository for WBS template persistence
// Implemented by Planova.Persistence.Repositories

public interface IWbsTemplateRepository
{
    Task<WbsTemplate> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<WbsTemplate>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<WbsTemplate>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<WbsTemplate> AddAsync(WbsTemplate template, CancellationToken ct);
    Task<WbsTemplate> UpdateAsync(WbsTemplate template, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
