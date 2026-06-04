// IBoqLibraryRepository — Repository for BOQ library aggregate persistence
// Implemented by Planova.Persistence.Repositories

public interface IBoqLibraryRepository
{
    Task<BoqLibrary> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoqLibrary>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<BoqLibrary>> GetByTypeAsync(LibraryType type, CancellationToken ct);
    Task<BoqLibrary> AddAsync(BoqLibrary library, CancellationToken ct);
    Task<BoqLibrary> UpdateAsync(BoqLibrary library, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
