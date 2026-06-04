namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsItemRepository
{
    Task<Entities.WbsItem> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsItem>> GetByWbsIdAsync(Guid wbsId, CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsItem>> GetChildrenAsync(Guid parentId, CancellationToken ct);
    Task<Entities.WbsItem> AddAsync(Entities.WbsItem item, CancellationToken ct);
    Task UpdateRangeAsync(IEnumerable<Entities.WbsItem> items, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
