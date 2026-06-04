namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsRepository
{
    Task<Entities.Wbs> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.Wbs>> GetByProjectIdAsync(int projectId, CancellationToken ct);
    Task<Entities.Wbs> AddAsync(Entities.Wbs wbs, CancellationToken ct);
    Task<Entities.Wbs> UpdateAsync(Entities.Wbs wbs, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
