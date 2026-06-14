using Microsoft.EntityFrameworkCore;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class PrimaveraValidationRepository : IPrimaveraValidationRepository
{
    private readonly PlanovaDbContext _context;

    public PrimaveraValidationRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrimaveraValidationIssue>> GetIssuesAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraValidationIssue>()
            .Where(i => i.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task AddIssuesAsync(IEnumerable<PrimaveraValidationIssue> issues, CancellationToken ct = default)
    {
        _context.Set<PrimaveraValidationIssue>().AddRange(issues);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ClearIssuesAsync(int projectId, CancellationToken ct = default)
    {
        var issues = await _context.Set<PrimaveraValidationIssue>()
            .Where(i => i.ProjectId == projectId)
            .ToListAsync(ct);
        _context.Set<PrimaveraValidationIssue>().RemoveRange(issues);
        await _context.SaveChangesAsync(ct);
    }
}
