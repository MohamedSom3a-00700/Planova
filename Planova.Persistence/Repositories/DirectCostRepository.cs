using Microsoft.EntityFrameworkCore;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class DirectCostRepository : IDirectCostRepository
{
    private readonly PlanovaDbContext _context;

    public DirectCostRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<DirectCost>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<DirectCost>()
            .Where(d => d.ProjectId == projectId)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<DirectCost>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        return await _context.Set<DirectCost>()
            .Where(d => d.ActivityId == activityId)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<DirectCost?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<DirectCost>()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task AddAsync(DirectCost directCost, CancellationToken ct = default)
    {
        _context.Set<DirectCost>().Add(directCost);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DirectCost directCost, CancellationToken ct = default)
    {
        _context.Set<DirectCost>().Update(directCost);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DirectCost directCost, CancellationToken ct = default)
    {
        _context.Set<DirectCost>().Remove(directCost);
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAsOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        var costs = await _context.Set<DirectCost>()
            .Where(d => d.ActivityId == activityId && !d.IsOrphaned)
            .ToListAsync(ct);

        foreach (var cost in costs)
        {
            cost.IsOrphaned = true;
            cost.DeletedActivityId = activityId;
        }

        await _context.SaveChangesAsync(ct);
    }
}
