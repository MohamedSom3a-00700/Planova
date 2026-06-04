namespace Planova.Boq.Domain.Interfaces;

public interface IBoqItemRepository
{
    Task<Entities.BoqItem> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.BoqItem>> GetByBoqIdAsync(Guid boqId, CancellationToken ct);
    Task<IReadOnlyList<Entities.BoqItem>> GetChildrenAsync(Guid parentId, CancellationToken ct);
    Task<Entities.BoqItem> AddAsync(Entities.BoqItem item, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<Entities.BoqItem> items, CancellationToken ct);
    Task<Entities.BoqItem> UpdateAsync(Entities.BoqItem item, CancellationToken ct);
    Task UpdateRangeAsync(IEnumerable<Entities.BoqItem> items, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<int> GetMaxSortOrderAsync(Guid boqId, Guid? parentId, CancellationToken ct);
}
