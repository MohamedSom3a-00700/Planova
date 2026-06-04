// IWbsRepository — Repository for WBS aggregate root persistence
// Implemented by Planova.Persistence.Repositories

public interface IWbsRepository
{
    Task<Wbs> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Wbs>> GetByProjectIdAsync(int projectId, CancellationToken ct);
    Task<Wbs> AddAsync(Wbs wbs, CancellationToken ct);
    Task<Wbs> UpdateAsync(Wbs wbs, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
