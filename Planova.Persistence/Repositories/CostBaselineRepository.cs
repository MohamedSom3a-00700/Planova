using Microsoft.EntityFrameworkCore;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class CostBaselineRepository : ICostBaselineRepository
{
    private readonly PlanovaDbContext _context;

    public CostBaselineRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<CostBaseline?> GetActiveBaselineAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<CostBaseline>()
            .Include(b => b.Rows)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.IsActive, ct);
    }

    public async Task<List<CostBaseline>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<CostBaseline>()
            .Where(b => b.ProjectId == projectId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<CostBaseline?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<CostBaseline>()
            .Include(b => b.Rows)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task AddAsync(CostBaseline baseline, CancellationToken ct = default)
    {
        _context.Set<CostBaseline>().Add(baseline);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CostBaseline baseline, CancellationToken ct = default)
    {
        _context.Set<CostBaseline>().Update(baseline);
        await _context.SaveChangesAsync(ct);
    }
}
