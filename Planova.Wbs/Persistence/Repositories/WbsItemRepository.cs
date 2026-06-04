using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Persistence.Repositories;

public class WbsItemRepository : IWbsItemRepository
{
    private readonly List<WbsItem> _store = new();
    private readonly object _lock = new();

    public Task<WbsItem> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            var entity = _store.FirstOrDefault(e => e.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"WbsItem {id} not found");
            return Task.FromResult(entity);
        }
    }

    public Task<IReadOnlyList<WbsItem>> GetByWbsIdAsync(Guid wbsId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<WbsItem>>(
                _store.Where(e => e.WbsId == wbsId)
                    .OrderBy(e => e.SortOrder)
                    .ToList());
        }
    }

    public Task<IReadOnlyList<WbsItem>> GetChildrenAsync(Guid parentId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<WbsItem>>(
                _store.Where(e => e.ParentId == parentId)
                    .OrderBy(e => e.SortOrder)
                    .ToList());
        }
    }

    public Task<WbsItem> AddAsync(WbsItem item, CancellationToken ct)
    {
        lock (_lock)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            _store.Add(item);
            return Task.FromResult(item);
        }
    }

    public Task UpdateRangeAsync(IEnumerable<WbsItem> items, CancellationToken ct)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                var existing = _store.FirstOrDefault(e => e.Id == item.Id);
                if (existing != null)
                {
                    _store.Remove(existing);
                    item.UpdatedAt = DateTime.UtcNow;
                    _store.Add(item);
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(e => e.Id == id);
        }
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(e => ids.Contains(e.Id));
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.Any(e => e.Id == id));
        }
    }
}
