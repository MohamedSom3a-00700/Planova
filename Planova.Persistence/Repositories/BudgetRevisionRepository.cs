using Microsoft.EntityFrameworkCore;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class BudgetRevisionRepository : IBudgetRevisionRepository
{
    private readonly PlanovaDbContext _context;

    public BudgetRevisionRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetRevision>> GetByBudgetIdAsync(Guid budgetId, CancellationToken ct = default)
    {
        return await _context.Set<BudgetRevision>()
            .Where(r => r.BudgetId == budgetId)
            .OrderByDescending(r => r.RevisionNumber)
            .ToListAsync(ct);
    }

    public async Task<BudgetRevision?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<BudgetRevision>()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<int> GetNextRevisionNumberAsync(Guid budgetId, CancellationToken ct = default)
    {
        var maxNumber = await _context.Set<BudgetRevision>()
            .Where(r => r.BudgetId == budgetId)
            .MaxAsync(r => (int?)r.RevisionNumber, ct);

        return (maxNumber ?? 0) + 1;
    }

    public async Task AddAsync(BudgetRevision revision, CancellationToken ct = default)
    {
        _context.Set<BudgetRevision>().Add(revision);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BudgetRevision revision, CancellationToken ct = default)
    {
        _context.Set<BudgetRevision>().Update(revision);
        await _context.SaveChangesAsync(ct);
    }
}
