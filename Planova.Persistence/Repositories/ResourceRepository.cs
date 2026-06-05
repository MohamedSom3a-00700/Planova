using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;
using Res = Planova.Resource.Domain.Entities.Resource;

namespace Planova.Persistence.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly PlanovaDbContext _context;

    public ResourceRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<Res?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Resources.FindAsync([id], ct);

    public async Task<List<Res>> GetByProjectAsync(int projectId, ResourceScope? scope = null, ResourceType? type = null, CancellationToken ct = default)
    {
        var query = _context.Resources.AsQueryable();
        if (scope.HasValue)
            query = query.Where(r => r.Scope == scope.Value || r.IsGlobal);
        else
            query = query.Where(r => r.ProjectId == projectId || r.IsGlobal);
        if (type.HasValue)
            query = query.Where(r => r.ResourceType == type.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<List<Res>> SearchAsync(string query, ResourceType? type = null, ResourceScope? scope = null, int? projectId = null, CancellationToken ct = default)
    {
        var q = _context.Resources.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(r => r.Name.Contains(query) || r.Code.Contains(query));
        if (type.HasValue)
            q = q.Where(r => r.ResourceType == type.Value);
        if (scope.HasValue)
            q = q.Where(r => r.Scope == scope.Value);
        if (projectId.HasValue)
            q = q.Where(r => r.ProjectId == projectId || r.IsGlobal);
        return await q.ToListAsync(ct);
    }

    public async Task<List<Res>> GetByTypeAsync(ResourceType type, ResourceScope? scope = null, int? projectId = null, CancellationToken ct = default)
    {
        var q = _context.Resources.Where(r => r.ResourceType == type);
        if (scope.HasValue)
            q = q.Where(r => r.Scope == scope.Value);
        if (projectId.HasValue)
            q = q.Where(r => r.ProjectId == projectId || r.IsGlobal);
        return await q.ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, ResourceScope scope, int? projectId = null, CancellationToken ct = default)
        => await _context.Resources.AnyAsync(r => r.Code == code && r.Scope == scope && r.ProjectId == projectId, ct);

    public async Task<string> GenerateNextCodeAsync(ResourceType type, CancellationToken ct = default)
    {
        var prefix = type switch
        {
            ResourceType.Labour => "R-LBR",
            ResourceType.Equipment => "R-EQP",
            ResourceType.Material => "R-MAT",
            ResourceType.Subcontractor => "R-SUB",
            _ => "R-SRC"
        };

        var maxCode = await _context.Resources
            .Where(r => r.Code.StartsWith(prefix))
            .OrderByDescending(r => r.Code)
            .Select(r => r.Code)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(maxCode))
            return $"{prefix}-001";

        var parts = maxCode.Split('-');
        if (parts.Length >= 3 && int.TryParse(parts[^1], out var num))
            return $"{prefix}-{(num + 1):D3}";

        return $"{prefix}-001";
    }

    public async Task<bool> HasDuplicateNameAsync(string name, ResourceScope scope, int? projectId = null, Guid? excludeId = null, CancellationToken ct = default)
    {
        var q = _context.Resources.Where(r => r.Name == name && r.Scope == scope && r.ProjectId == projectId);
        if (excludeId.HasValue)
            q = q.Where(r => r.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }

    public async Task AddAsync(Res resource, CancellationToken ct = default)
    {
        await _context.Resources.AddAsync(resource, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Res resource, CancellationToken ct = default)
    {
        _context.Resources.Update(resource);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Resources.FindAsync([id], ct);
        if (entity is not null)
        {
            _context.Resources.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> GetCountAsync(ResourceType? type = null, ResourceScope? scope = null, CancellationToken ct = default)
    {
        var q = _context.Resources.AsQueryable();
        if (type.HasValue)
            q = q.Where(r => r.ResourceType == type.Value);
        if (scope.HasValue)
            q = q.Where(r => r.Scope == scope.Value);
        return await q.CountAsync(ct);
    }
}
