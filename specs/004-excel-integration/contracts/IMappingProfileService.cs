// IMappingProfileService — CRUD operations for saved column mapping profiles
// Implemented by Planova.Excel.Services

public interface IMappingProfileService
{
    Task<MappingProfile> CreateAsync(string name, string entityType, Dictionary<string, string> columnMappings, CancellationToken ct);
    Task<MappingProfile> UpdateAsync(Guid id, string name, Dictionary<string, string> columnMappings, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<MappingProfile> CloneAsync(Guid id, string newName, CancellationToken ct);
    Task<MappingProfile> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<MappingProfile>> GetAllAsync(string entityType, CancellationToken ct);
    Task<Dictionary<string, string>> ApplyAsync(Guid profileId, IReadOnlyList<string> detectedColumns, CancellationToken ct);
}
