using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

public class ActivityRepository : IActivityRepository
{
    private readonly PlanovaDbContext _context;

    public ActivityRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<ActivityEntity>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Activities
            .Where(a => a.ProjectId == projectId)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Code)
            .ToListAsync(ct);
    }

    public async Task<List<ActivityEntity>> GetByWbsItemIdAsync(Guid wbsItemId, CancellationToken ct = default)
    {
        return await _context.Activities
            .Where(a => a.WbsItemId == wbsItemId)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Code)
            .ToListAsync(ct);
    }

    public async Task<List<ActivityEntity>> GetByStatusAsync(int projectId, ActivityStatus status, CancellationToken ct = default)
    {
        return await _context.Activities
            .Where(a => a.ProjectId == projectId && a.Status == status)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Code)
            .ToListAsync(ct);
    }

    public async Task<List<ActivityEntity>> GetChildrenAsync(Guid parentActivityId, CancellationToken ct = default)
    {
        return await _context.Activities
            .Where(a => a.ParentActivityId == parentActivityId)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Code)
            .ToListAsync(ct);
    }

    public async Task<string> GetNextCodeAsync(int projectId, CancellationToken ct = default)
    {
        var maxCode = await _context.Activities
            .Where(a => a.ProjectId == projectId)
            .MaxAsync(a => (string?)a.Code, ct);

        if (string.IsNullOrEmpty(maxCode))
            return "A-001";

        var parts = maxCode.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var num))
            return $"A-{(num + 1):D3}";

        return "A-001";
    }

    public async Task AddAsync(ActivityEntity activity, CancellationToken ct = default)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ActivityEntity activity, CancellationToken ct = default)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Activities.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.Activities.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Activities.AnyAsync(a => a.Id == id, ct);
    }
}
