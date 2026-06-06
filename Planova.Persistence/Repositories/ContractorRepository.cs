using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ContractorRepository : IContractorRepository
{
    private readonly PlanovaDbContext _context;

    public ContractorRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contractor>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<Contractor>()
            .Include(c => c.Projects)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Contractor?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var contractor = await _context.Set<Contractor>()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (contractor != null)
        {
            await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.ContractorId == id)
                .LoadAsync(ct);
        }

        return contractor;
    }

    public async Task<Contractor> AddAsync(Contractor contractor, CancellationToken ct = default)
    {
        _context.Set<Contractor>().Add(contractor);
        await _context.SaveChangesAsync(ct);
        return contractor;
    }

    public async Task UpdateAsync(Contractor contractor, CancellationToken ct = default)
    {
        _context.Set<Contractor>().Update(contractor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Contractor contractor, CancellationToken ct = default)
    {
        _context.Set<Contractor>().Remove(contractor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Contractor>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.ToLower();
        return await _context.Set<Contractor>()
            .Include(c => c.Projects)
            .Where(c => c.Name.ToLower().Contains(q) || c.Code.ToLower().Contains(q))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Contractor>()
            .AnyAsync(c => c.Code == code && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Contractor>()
            .AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await _context.Set<Contractor>().CountAsync(ct);
    }

    public async Task<bool> HasLinkedProjectsAsync(int contractorId, CancellationToken ct = default)
    {
        return await _context.Projects.AnyAsync(p => p.ContractorId == contractorId, ct);
    }
}