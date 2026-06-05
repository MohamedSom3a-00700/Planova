using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ResourceAssignmentRepository : IResourceAssignmentRepository
{
    private readonly PlanovaDbContext _context;

    public ResourceAssignmentRepository(PlanovaDbContext context) => _context = context;

    public async Task<ResourceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ResourceAssignments.Include(ra => ra.Resource).FirstOrDefaultAsync(ra => ra.Id == id, ct);

    public async Task<List<ResourceAssignment>> GetByActivityAsync(Guid activityId, CancellationToken ct = default)
        => await _context.ResourceAssignments.Where(ra => ra.ActivityId == activityId).Include(ra => ra.Resource).ToListAsync(ct);

    public async Task<List<ResourceAssignment>> GetByProjectAsync(int projectId, CancellationToken ct = default)
        => await _context.ResourceAssignments.Where(ra => ra.ProjectId == projectId).Include(ra => ra.Resource).ToListAsync(ct);

    public async Task<List<ResourceAssignment>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default)
        => await _context.ResourceAssignments.Where(ra => ra.ResourceId == resourceId).ToListAsync(ct);

    public async Task<bool> HasAssignmentsForActivityAsync(Guid activityId, CancellationToken ct = default)
        => await _context.ResourceAssignments.AnyAsync(ra => ra.ActivityId == activityId, ct);

    public async Task<bool> HasAssignmentsForResourceAsync(Guid resourceId, CancellationToken ct = default)
        => await _context.ResourceAssignments.AnyAsync(ra => ra.ResourceId == resourceId, ct);

    public async Task AddAsync(ResourceAssignment assignment, CancellationToken ct = default)
    {
        await _context.ResourceAssignments.AddAsync(assignment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ResourceAssignment> assignments, CancellationToken ct = default)
    {
        await _context.ResourceAssignments.AddRangeAsync(assignments, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ResourceAssignment assignment, CancellationToken ct = default)
    {
        _context.ResourceAssignments.Update(assignment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.ResourceAssignments.FindAsync([id], ct);
        if (entity is not null)
        {
            _context.ResourceAssignments.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<decimal> GetTotalCostForActivityAsync(Guid activityId, CancellationToken ct = default)
        => await _context.ResourceAssignments.Where(ra => ra.ActivityId == activityId).SumAsync(ra => ra.TotalCost, ct);
}
