using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class SubcontractorRepository : ISubcontractorRepository
{
    private readonly PlanovaDbContext _context;

    public SubcontractorRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Subcontractor>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<Subcontractor>()
            .Include(s => s.Projects)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Subcontractor?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var subcontractor = await _context.Set<Subcontractor>()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (subcontractor != null)
        {
            await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.SubcontractorId == id)
                .LoadAsync(ct);
        }

        return subcontractor;
    }

    public async Task<Subcontractor> AddAsync(Subcontractor subcontractor, CancellationToken ct = default)
    {
        _context.Set<Subcontractor>().Add(subcontractor);
        await _context.SaveChangesAsync(ct);
        return subcontractor;
    }

    public async Task UpdateAsync(Subcontractor subcontractor, CancellationToken ct = default)
    {
        _context.Set<Subcontractor>().Update(subcontractor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Subcontractor subcontractor, CancellationToken ct = default)
    {
        _context.Set<Subcontractor>().Remove(subcontractor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Subcontractor>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.ToLower();
        return await _context.Set<Subcontractor>()
            .Include(s => s.Projects)
            .Where(s => s.Name.ToLower().Contains(q) || s.Code.ToLower().Contains(q))
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Subcontractor>()
            .AnyAsync(s => s.Code == code && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Subcontractor>()
            .AnyAsync(s => s.Name == name && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await _context.Set<Subcontractor>().CountAsync(ct);
    }

    public async Task<bool> HasLinkedProjectsAsync(int subcontractorId, CancellationToken ct = default)
    {
        return await _context.Projects.AnyAsync(p => p.SubcontractorId == subcontractorId, ct);
    }
}