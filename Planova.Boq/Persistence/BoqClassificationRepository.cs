using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Persistence;

public class BoqClassificationRepository : IBoqClassificationRepository
{
    private readonly List<BoqClassification> _store = new();
    private readonly object _lock = new();

    public Task<BoqClassification> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.FirstOrDefault(c => c.Id == id) ?? new BoqClassification { Id = id, Code = "N/A", Name = "Unknown" });
        }
    }

    public Task<IReadOnlyList<BoqClassification>> GetByProjectIdAsync(Guid? projectId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqClassification>>(_store.Where(c => c.ProjectId == projectId).ToList());
        }
    }

    public Task<IReadOnlyList<BoqClassification>> GetGlobalAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqClassification>>(_store.Where(c => c.Scope == ClassificationScope.Global).ToList());
        }
    }

    public Task<IReadOnlyList<BoqClassification>> GetChildrenAsync(Guid parentId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqClassification>>(_store.Where(c => c.ParentId == parentId).ToList());
        }
    }

    public Task<BoqClassification> AddAsync(BoqClassification classification, CancellationToken ct)
    {
        lock (_lock)
        {
            classification.Id = Guid.NewGuid();
            _store.Add(classification);
            return Task.FromResult(classification);
        }
    }

    public Task<BoqClassification> UpdateAsync(BoqClassification classification, CancellationToken ct)
    {
        lock (_lock)
        {
            var existing = _store.FirstOrDefault(c => c.Id == classification.Id);
            if (existing is not null)
            {
                _store.Remove(existing);
                _store.Add(classification);
            }
            return Task.FromResult(classification);
        }
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(c => c.Id == id);
        }
        return Task.CompletedTask;
    }
}
