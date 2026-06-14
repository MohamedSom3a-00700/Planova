using Microsoft.EntityFrameworkCore;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class PrimaveraRepairRepository : IPrimaveraRepairRepository
{
    private readonly PlanovaDbContext _context;

    public PrimaveraRepairRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrimaveraRepairAction>> GetActionsAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraRepairAction>()
            .Where(a => a.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<PrimaveraRepairAction> AddActionAsync(PrimaveraRepairAction action, CancellationToken ct = default)
    {
        _context.Set<PrimaveraRepairAction>().Add(action);
        await _context.SaveChangesAsync(ct);
        return action;
    }

    public async Task UpdateActionAsync(PrimaveraRepairAction action, CancellationToken ct = default)
    {
        _context.Set<PrimaveraRepairAction>().Update(action);
        await _context.SaveChangesAsync(ct);
    }
}
