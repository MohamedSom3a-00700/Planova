using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ResourceUsageRepository : IResourceUsageRepository
{
    private readonly PlanovaDbContext _context;

    public ResourceUsageRepository(PlanovaDbContext context) => _context = context;

    public async Task<List<ResourceUsage>> GetByResourceAsync(Guid resourceId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var q = _context.ResourceUsages.Where(ru => ru.ResourceId == resourceId);
        if (from.HasValue) q = q.Where(ru => ru.Date >= from.Value);
        if (to.HasValue) q = q.Where(ru => ru.Date <= to.Value);
        return await q.ToListAsync(ct);
    }

    public async Task<List<ResourceUsage>> GetByProjectAsync(int projectId, DateTime? from = null, DateTime? to = null, ResourceType? typeFilter = null, CancellationToken ct = default)
    {
        var q = from ru in _context.ResourceUsages
                join ra in _context.ResourceAssignments on ru.AssignmentId equals ra.Id
                where ra.ProjectId == projectId
                select ru;

        if (from.HasValue) q = q.Where(ru => ru.Date >= from.Value);
        if (to.HasValue) q = q.Where(ru => ru.Date <= to.Value);
        if (typeFilter.HasValue)
            q = q.Where(ru => ru.Resource.ResourceType == typeFilter.Value);

        return await q.ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ResourceUsage> usages, CancellationToken ct = default)
    {
        await _context.ResourceUsages.AddRangeAsync(usages, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteByAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var items = await _context.ResourceUsages.Where(ru => ru.AssignmentId == assignmentId).ToListAsync(ct);
        _context.ResourceUsages.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RegenerateForAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        await DeleteByAssignmentAsync(assignmentId, ct);
        var assignment = await _context.ResourceAssignments.FindAsync([assignmentId], ct);
        if (assignment is null || !assignment.StartDate.HasValue || !assignment.EndDate.HasValue)
            return;

        var totalDays = (assignment.EndDate.Value - assignment.StartDate.Value).Days + 1;
        if (totalDays <= 0) return;

        var dailyQuantity = assignment.Quantity / totalDays;
        var usages = new List<ResourceUsage>();
        for (var date = assignment.StartDate.Value; date <= assignment.EndDate.Value; date = date.AddDays(1))
        {
            usages.Add(new ResourceUsage
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                ResourceId = assignment.ResourceId,
                Date = date,
                PlannedQuantity = Math.Round(dailyQuantity, 4)
            });
        }
        await AddRangeAsync(usages, ct);
    }
}
