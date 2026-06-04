using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Persistence.Repositories;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public class WbsRepository : IWbsRepository
{
    private readonly List<WbsEntity> _store = new();
    private readonly object _lock = new();

    public Task<WbsEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            var entity = _store.FirstOrDefault(e => e.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"WBS {id} not found");
            return Task.FromResult(entity);
        }
    }

    public Task<IReadOnlyList<WbsEntity>> GetByProjectIdAsync(int projectId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<WbsEntity>>(
                _store.Where(e => e.ProjectId == projectId)
                    .OrderBy(e => e.Name)
                    .ToList());
        }
    }

    public Task<WbsEntity> AddAsync(WbsEntity wbs, CancellationToken ct)
    {
        lock (_lock)
        {
            wbs.Id = Guid.NewGuid();
            wbs.CreatedAt = DateTime.UtcNow;
            wbs.UpdatedAt = DateTime.UtcNow;
            _store.Add(wbs);
            return Task.FromResult(wbs);
        }
    }

    public Task<WbsEntity> UpdateAsync(WbsEntity wbs, CancellationToken ct)
    {
        lock (_lock)
        {
            var existing = _store.FirstOrDefault(e => e.Id == wbs.Id);
            if (existing != null)
            {
                _store.Remove(existing);
                wbs.UpdatedAt = DateTime.UtcNow;
                _store.Add(wbs);
            }
            return Task.FromResult(wbs);
        }
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(e => e.Id == id);
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
