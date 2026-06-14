using Microsoft.EntityFrameworkCore;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class PrimaveraWorkspaceRepository : IPrimaveraWorkspaceRepository
{
    private readonly PlanovaDbContext _context;

    public PrimaveraWorkspaceRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrimaveraActivity>> GetActivitiesAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraActivity>()
            .Where(a => a.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<PrimaveraRelationship>> GetRelationshipsAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraRelationship>()
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<PrimaveraResourceAssignment>> GetResourceAssignmentsAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraResourceAssignment>()
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<PrimaveraCalendar>> GetCalendarsAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraCalendar>()
            .Where(c => c.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<PrimaveraCode>> GetCodesAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraCode>()
            .Where(c => c.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<PrimaveraBaseline>> GetBaselinesAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraBaseline>()
            .Where(b => b.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<List<PrimaveraUdf>> GetUdfsAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraUdf>()
            .Where(u => u.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task UpdateActivityAsync(PrimaveraActivity activity, CancellationToken ct = default)
    {
        _context.Set<PrimaveraActivity>().Update(activity);
        await Task.CompletedTask;
    }

    public async Task UpdateRelationshipAsync(PrimaveraRelationship relationship, CancellationToken ct = default)
    {
        _context.Set<PrimaveraRelationship>().Update(relationship);
        await Task.CompletedTask;
    }

    public async Task UpdateResourceAssignmentAsync(PrimaveraResourceAssignment assignment, CancellationToken ct = default)
    {
        _context.Set<PrimaveraResourceAssignment>().Update(assignment);
        await Task.CompletedTask;
    }

    public async Task UpdateCalendarAsync(PrimaveraCalendar calendar, CancellationToken ct = default)
    {
        _context.Set<PrimaveraCalendar>().Update(calendar);
        await Task.CompletedTask;
    }

    public async Task UpdateCodeAsync(PrimaveraCode code, CancellationToken ct = default)
    {
        _context.Set<PrimaveraCode>().Update(code);
        await Task.CompletedTask;
    }

    public async Task UpdateBaselineAsync(PrimaveraBaseline baseline, CancellationToken ct = default)
    {
        _context.Set<PrimaveraBaseline>().Update(baseline);
        await Task.CompletedTask;
    }

    public async Task UpdateUdfAsync(PrimaveraUdf udf, CancellationToken ct = default)
    {
        _context.Set<PrimaveraUdf>().Update(udf);
        await Task.CompletedTask;
    }

    public async Task<PrimaveraActivity?> GetActivityByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraActivity>().FindAsync(new object[] { id }, ct);
    }

    public async Task<PrimaveraRelationship?> GetRelationshipByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraRelationship>().FindAsync(new object[] { id }, ct);
    }

    public async Task<PrimaveraResourceAssignment?> GetResourceAssignmentByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraResourceAssignment>().FindAsync(new object[] { id }, ct);
    }

    public async Task<PrimaveraCalendar?> GetCalendarByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraCalendar>().FindAsync(new object[] { id }, ct);
    }

    public async Task<PrimaveraCode?> GetCodeByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraCode>().FindAsync(new object[] { id }, ct);
    }

    public async Task<PrimaveraBaseline?> GetBaselineByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraBaseline>().FindAsync(new object[] { id }, ct);
    }

    public async Task<PrimaveraUdf?> GetUdfByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraUdf>().FindAsync(new object[] { id }, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
