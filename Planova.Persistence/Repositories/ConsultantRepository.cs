using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ConsultantRepository : IConsultantRepository
{
    private readonly PlanovaDbContext _context;

    public ConsultantRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Consultant>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<Consultant>()
            .Include(c => c.Projects)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Consultant?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var consultant = await _context.Set<Consultant>()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (consultant != null)
        {
            await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.ConsultantId == id)
                .LoadAsync(ct);
        }

        return consultant;
    }

    public async Task<Consultant> AddAsync(Consultant consultant, CancellationToken ct = default)
    {
        _context.Set<Consultant>().Add(consultant);
        await _context.SaveChangesAsync(ct);
        return consultant;
    }

    public async Task UpdateAsync(Consultant consultant, CancellationToken ct = default)
    {
        _context.Set<Consultant>().Update(consultant);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Consultant consultant, CancellationToken ct = default)
    {
        _context.Set<Consultant>().Remove(consultant);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Consultant>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.ToLower();
        return await _context.Set<Consultant>()
            .Include(c => c.Projects)
            .Where(c => c.Name.ToLower().Contains(q) || c.Code.ToLower().Contains(q))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Consultant>()
            .AnyAsync(c => c.Code == code && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Set<Consultant>()
            .AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await _context.Set<Consultant>().CountAsync(ct);
    }

    public async Task<bool> HasLinkedProjectsAsync(int consultantId, CancellationToken ct = default)
    {
        return await _context.Projects.AnyAsync(p => p.ConsultantId == consultantId, ct);
    }
}
