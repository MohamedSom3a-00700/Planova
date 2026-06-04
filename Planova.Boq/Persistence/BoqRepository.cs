using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Persistence;

using BoqEntity = Planova.Boq.Domain.Entities.Boq;

public class BoqRepository : IBoqRepository
{
    private readonly List<BoqEntity> _store = new();
    private readonly object _lock = new();

    public Task<BoqEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            var boq = _store.FirstOrDefault(b => b.Id == id);
            return Task.FromResult(boq ?? new BoqEntity { Id = id, Name = "New BOQ", Currency = "USD" });
        }
    }

    public Task<IReadOnlyList<BoqEntity>> GetByProjectIdAsync(Guid projectId, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqEntity>>(_store.Where(b => b.ProjectId == projectId).ToList());
        }
    }

    public Task<BoqEntity> AddAsync(BoqEntity boq, CancellationToken ct)
    {
        lock (_lock)
        {
            boq.Id = Guid.NewGuid();
            boq.CreatedAt = DateTime.UtcNow;
            boq.ModifiedAt = DateTime.UtcNow;
            _store.Add(boq);
            return Task.FromResult(boq);
        }
    }

    public Task<BoqEntity> UpdateAsync(BoqEntity boq, CancellationToken ct)
    {
        lock (_lock)
        {
            var existing = _store.FirstOrDefault(b => b.Id == boq.Id);
            if (existing is not null)
            {
                _store.Remove(existing);
                _store.Add(boq);
            }
            return Task.FromResult(boq);
        }
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(b => b.Id == id);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.Any(b => b.Id == id));
        }
    }
}
