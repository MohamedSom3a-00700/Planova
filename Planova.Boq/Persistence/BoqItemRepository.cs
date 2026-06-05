using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Persistence;

public class BoqItemRepository : IBoqItemRepository
{
    private readonly List<BoqItem> _store = new();
    private readonly object _lock = new();

    public Task<BoqItem> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            var item = _store.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(item ?? new BoqItem { Id = id, BoqId = Guid.Empty, Code = "N/A", Description = string.Empty });
        }
    }

    public Task<IReadOnlyList<BoqItem>> GetByBoqIdAsync(Guid boqId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqItem>>(_store.Where(i => i.BoqId == boqId).ToList());
        }
    }

    public Task<IReadOnlyList<BoqItem>> GetChildrenAsync(Guid parentId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqItem>>(_store.Where(i => i.ParentId == parentId).ToList());
        }
    }

    public Task<BoqItem> AddAsync(BoqItem item, CancellationToken ct)
    {
        lock (_lock)
        {
            item.Id = Guid.NewGuid();
            _store.Add(item);
            return Task.FromResult(item);
        }
    }

    public Task AddRangeAsync(IEnumerable<BoqItem> items, CancellationToken ct)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                item.Id = Guid.NewGuid();
                _store.Add(item);
            }
        }
        return Task.CompletedTask;
    }

    public Task<BoqItem> UpdateAsync(BoqItem item, CancellationToken ct)
    {
        lock (_lock)
        {
            var existing = _store.FirstOrDefault(i => i.Id == item.Id);
            if (existing is not null)
            {
                _store.Remove(existing);
                _store.Add(item);
            }
            return Task.FromResult(item);
        }
    }

    public Task UpdateRangeAsync(IEnumerable<BoqItem> items, CancellationToken ct)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                var existing = _store.FirstOrDefault(i => i.Id == item.Id);
                if (existing is not null)
                {
                    _store.Remove(existing);
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
            _store.RemoveAll(i => i.Id == id);
        }
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(i => ids.Contains(i.Id));
        }
        return Task.CompletedTask;
    }

    public Task<int> GetMaxSortOrderAsync(Guid boqId, Guid? parentId, CancellationToken ct)
    {
        lock (_lock)
        {
            var max = _store
                .Where(i => i.BoqId == boqId && i.ParentId == parentId)
                .Select(i => (int?)i.SortOrder)
                .Max() ?? -1;
            return Task.FromResult(max + 1);
        }
    }
}
