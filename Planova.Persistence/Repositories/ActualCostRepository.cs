using Microsoft.EntityFrameworkCore;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ActualCostRepository : IActualCostRepository
{
    private readonly PlanovaDbContext _context;

    public ActualCostRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ActualCost?> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        return await _context.Set<ActualCost>()
            .FirstOrDefaultAsync(a => a.ActivityId == activityId, ct);
    }

    public async Task<List<ActualCost>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<ActualCost>()
            .Where(a => a.ProjectId == projectId)
            .OrderBy(a => a.EntryDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ActualCost actualCost, CancellationToken ct = default)
    {
        _context.Set<ActualCost>().Add(actualCost);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ActualCost actualCost, CancellationToken ct = default)
    {
        _context.Set<ActualCost>().Update(actualCost);
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAsOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        var costs = await _context.Set<ActualCost>()
            .Where(a => a.ActivityId == activityId && !a.IsOrphaned)
            .ToListAsync(ct);

        foreach (var cost in costs)
        {
            cost.IsOrphaned = true;
            cost.DeletedActivityId = activityId;
        }

        await _context.SaveChangesAsync(ct);
    }
}
