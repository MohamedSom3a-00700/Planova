using Microsoft.EntityFrameworkCore;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly PlanovaDbContext _context;

    public BudgetRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<Budget>()
            .Include(b => b.Revisions)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, ct);
    }

    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Budget>()
            .Include(b => b.Revisions)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task AddAsync(Budget budget, CancellationToken ct = default)
    {
        _context.Set<Budget>().Add(budget);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Budget budget, CancellationToken ct = default)
    {
        _context.Set<Budget>().Update(budget);
        await _context.SaveChangesAsync(ct);
    }
}
