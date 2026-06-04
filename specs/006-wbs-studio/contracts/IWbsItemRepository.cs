// IWbsItemRepository — Repository for WBS item tree persistence
// Implemented by Planova.Persistence.Repositories

public interface IWbsItemRepository
{
    Task<WbsItem> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<WbsItem>> GetByWbsIdAsync(Guid wbsId, CancellationToken ct);
    Task<IReadOnlyList<WbsItem>> GetChildrenAsync(Guid parentId, CancellationToken ct);
    Task<WbsItem> AddAsync(WbsItem item, CancellationToken ct);
    Task UpdateRangeAsync(IEnumerable<WbsItem> items, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
