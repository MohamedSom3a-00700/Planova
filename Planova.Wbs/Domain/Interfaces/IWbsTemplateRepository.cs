namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsTemplateRepository
{
    Task<Entities.WbsTemplate> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsTemplate>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsTemplate>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<Entities.WbsTemplate> AddAsync(Entities.WbsTemplate template, CancellationToken ct);
    Task<Entities.WbsTemplate> UpdateAsync(Entities.WbsTemplate template, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
