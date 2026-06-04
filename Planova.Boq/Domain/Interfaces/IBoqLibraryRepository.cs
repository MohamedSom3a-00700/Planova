using Planova.Boq.Domain.Entities;

namespace Planova.Boq.Domain.Interfaces;

public interface IBoqLibraryRepository
{
    Task<BoqLibrary> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoqLibrary>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<BoqLibrary>> GetByTypeAsync(Enums.LibraryType type, CancellationToken ct);
    Task<BoqLibrary> AddAsync(BoqLibrary library, CancellationToken ct);
    Task<BoqLibrary> UpdateAsync(BoqLibrary library, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
