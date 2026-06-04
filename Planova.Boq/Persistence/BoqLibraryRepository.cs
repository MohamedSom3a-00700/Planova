using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Persistence;

public class BoqLibraryRepository : IBoqLibraryRepository
{
    private readonly List<BoqLibrary> _store = new();
    private readonly object _lock = new();

    public Task<BoqLibrary> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.FirstOrDefault(l => l.Id == id) ?? new BoqLibrary { Id = id, Name = "New Library" });
        }
    }

    public Task<IReadOnlyList<BoqLibrary>> GetAllAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqLibrary>>(_store.ToList());
        }
    }

    public Task<IReadOnlyList<BoqLibrary>> GetByTypeAsync(LibraryType type, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<BoqLibrary>>(_store.Where(l => l.LibraryType == type).ToList());
        }
    }

    public Task<BoqLibrary> AddAsync(BoqLibrary library, CancellationToken ct)
    {
        lock (_lock)
        {
            library.Id = Guid.NewGuid();
            library.CreatedAt = DateTime.UtcNow;
            library.ModifiedAt = DateTime.UtcNow;
            _store.Add(library);
            return Task.FromResult(library);
        }
    }

    public Task<BoqLibrary> UpdateAsync(BoqLibrary library, CancellationToken ct)
    {
        lock (_lock)
        {
            var existing = _store.FirstOrDefault(l => l.Id == library.Id);
            if (existing is not null)
            {
                _store.Remove(existing);
                _store.Add(library);
            }
            return Task.FromResult(library);
        }
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(l => l.Id == id);
        }
        return Task.CompletedTask;
    }
}
