// IBoqItemRepository — Repository for BOQ item persistence and tree operations
// Implemented by Planova.Persistence.Repositories

public interface IBoqItemRepository
{
    Task<BoqItem> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoqItem>> GetByBoqIdAsync(Guid boqId, CancellationToken ct);
    Task<IReadOnlyList<BoqItem>> GetChildrenAsync(Guid parentId, CancellationToken ct);
    Task<BoqItem> AddAsync(BoqItem item, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<BoqItem> items, CancellationToken ct);
    Task<BoqItem> UpdateAsync(BoqItem item, CancellationToken ct);
    Task UpdateRangeAsync(IEnumerable<BoqItem> items, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<int> GetMaxSortOrderAsync(Guid boqId, Guid? parentId, CancellationToken ct);
}
