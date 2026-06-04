namespace Planova.Boq.Domain.Interfaces;

public interface IBoqRepository
{
    Task<Entities.Boq> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.Boq>> GetByProjectIdAsync(Guid projectId, CancellationToken ct);
    Task<Entities.Boq> AddAsync(Entities.Boq boq, CancellationToken ct);
    Task<Entities.Boq> UpdateAsync(Entities.Boq boq, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
