using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ActivityRelationshipRepository : IActivityRelationshipRepository
{
    private readonly PlanovaDbContext _context;

    public ActivityRelationshipRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityRelationship?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ActivityRelationships
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<ActivityRelationship>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ActivityRelationships
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<ActivityRelationship>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        return await _context.ActivityRelationships
            .Where(r => r.PredecessorId == activityId || r.SuccessorId == activityId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ActivityRelationship relationship, CancellationToken ct = default)
    {
        _context.ActivityRelationships.Add(relationship);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ActivityRelationship relationship, CancellationToken ct = default)
    {
        _context.ActivityRelationships.Update(relationship);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.ActivityRelationships
            .FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.ActivityRelationships.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid predecessorId, Guid successorId, RelationshipType type, CancellationToken ct = default)
    {
        return await _context.ActivityRelationships
            .AnyAsync(r => r.PredecessorId == predecessorId
                && r.SuccessorId == successorId
                && r.Type == type, ct);
    }
}
