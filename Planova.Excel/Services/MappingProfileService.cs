using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Excel.Models;
using Planova.Persistence.DbContext;

namespace Planova.Excel.Services;

public class MappingProfileService : IMappingProfileService
{
    private readonly PlanovaDbContext _dbContext;

    public MappingProfileService(PlanovaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MappingProfile> CreateAsync(string name, string entityType, Dictionary<string, string> columnMappings, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = new ExcelMappingProfile
        {
            Id = Guid.NewGuid(),
            Name = name,
            EntityType = entityType,
            Version = 1,
            ColumnMappingsJson = JsonSerializer.Serialize(columnMappings),
            ValidationRulesJson = "[]",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        _dbContext.ExcelMappingProfiles.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return MapToModel(entity);
    }

    public async Task<MappingProfile> UpdateAsync(Guid id, string name, Dictionary<string, string> columnMappings, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = await _dbContext.ExcelMappingProfiles.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Mapping profile {id} not found.");

        entity.Name = name;
        entity.ColumnMappingsJson = JsonSerializer.Serialize(columnMappings);
        entity.Version++;
        entity.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        return MapToModel(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = await _dbContext.ExcelMappingProfiles.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Mapping profile {id} not found.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<MappingProfile> CloneAsync(Guid id, string newName, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var source = await _dbContext.ExcelMappingProfiles.FindAsync(new object[] { id }, ct);
        if (source is null)
            throw new KeyNotFoundException($"Mapping profile {id} not found.");

        var clone = new ExcelMappingProfile
        {
            Id = Guid.NewGuid(),
            Name = newName,
            EntityType = source.EntityType,
            Version = 1,
            ColumnMappingsJson = source.ColumnMappingsJson,
            ValidationRulesJson = source.ValidationRulesJson,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        _dbContext.ExcelMappingProfiles.Add(clone);
        await _dbContext.SaveChangesAsync(ct);

        return MapToModel(clone);
    }

    public async Task<MappingProfile> GetAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = await _dbContext.ExcelMappingProfiles.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Mapping profile {id} not found.");

        return MapToModel(entity);
    }

    public async Task<IReadOnlyList<MappingProfile>> GetAllAsync(string entityType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entities = await _dbContext.ExcelMappingProfiles
            .Where(p => p.EntityType == entityType)
            .OrderByDescending(p => p.ModifiedAt)
            .ToListAsync(ct);

        return entities.Select(MapToModel).ToList();
    }

    public Task<Dictionary<string, string>> ApplyAsync(Guid profileId, IReadOnlyList<string> detectedColumns, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // In a full implementation, this would auto-map detected columns to the profile's mappings
        return Task.FromResult(new Dictionary<string, string>());
    }

    private static MappingProfile MapToModel(ExcelMappingProfile entity)
    {
        return new MappingProfile
        {
            Id = entity.Id,
            Name = entity.Name,
            EntityType = entity.EntityType,
            Version = entity.Version,
            ColumnMappings = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.ColumnMappingsJson) ?? new(),
            ValidationRules = JsonSerializer.Deserialize<List<string>>(entity.ValidationRulesJson) ?? new(),
            CreatedAt = entity.CreatedAt,
            ModifiedAt = entity.ModifiedAt
        };
    }
}
