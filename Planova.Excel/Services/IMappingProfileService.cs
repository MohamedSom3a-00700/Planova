using Planova.Excel.Models;

namespace Planova.Excel.Services;

/// <summary>CRUD operations for saved column mapping profiles.</summary>
public interface IMappingProfileService
{
    /// <summary>Creates a new mapping profile.</summary>
    Task<MappingProfile> CreateAsync(string name, string entityType, Dictionary<string, string> columnMappings, CancellationToken ct);

    /// <summary>Updates an existing mapping profile's name and mappings.</summary>
    Task<MappingProfile> UpdateAsync(Guid id, string name, Dictionary<string, string> columnMappings, CancellationToken ct);

    /// <summary>Soft-deletes a mapping profile.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct);

    /// <summary>Creates a copy of an existing mapping profile with a new name.</summary>
    Task<MappingProfile> CloneAsync(Guid id, string newName, CancellationToken ct);

    /// <summary>Gets a single mapping profile by ID.</summary>
    Task<MappingProfile> GetAsync(Guid id, CancellationToken ct);

    /// <summary>Gets all mapping profiles for a given entity type.</summary>
    Task<IReadOnlyList<MappingProfile>> GetAllAsync(string entityType, CancellationToken ct);

    /// <summary>Applies a profile's column mappings against detected columns.</summary>
    Task<Dictionary<string, string>> ApplyAsync(Guid profileId, IReadOnlyList<string> detectedColumns, CancellationToken ct);
}
