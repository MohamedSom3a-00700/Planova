using Planova.Boq.Domain.Entities;

namespace Planova.Boq.Domain.Interfaces;

public interface IBoqClassificationRepository
{
    Task<BoqClassification> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoqClassification>> GetByProjectIdAsync(Guid? projectId, CancellationToken ct);
    Task<IReadOnlyList<BoqClassification>> GetGlobalAsync(CancellationToken ct);
    Task<IReadOnlyList<BoqClassification>> GetChildrenAsync(Guid parentId, CancellationToken ct);
    Task<BoqClassification> AddAsync(BoqClassification classification, CancellationToken ct);
    Task<BoqClassification> UpdateAsync(BoqClassification classification, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
