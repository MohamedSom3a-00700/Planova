// IBoqRepository — Repository for BOQ aggregate root persistence
// Implemented by Planova.Persistence.Repositories

public interface IBoqRepository
{
    Task<Boq> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Boq>> GetByProjectIdAsync(Guid projectId, CancellationToken ct);
    Task<Boq> AddAsync(Boq boq, CancellationToken ct);
    Task<Boq> UpdateAsync(Boq boq, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
