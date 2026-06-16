using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ComparisonRepository : IComparisonRepository
{
    private readonly PlanovaDbContext _context;

    public ComparisonRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ComparisonSession?> GetSessionByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ComparisonSessions.FindAsync(new object[] { id }, ct);

    public async Task<List<ComparisonSession>> GetSessionsByProjectAsync(int projectId, CancellationToken ct = default)
        => await _context.ComparisonSessions
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<ComparisonResult>> GetResultsBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
        => await _context.ComparisonResults
            .Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.EntityType)
            .ThenBy(r => r.MatchKey)
            .ToListAsync(ct);

    public async Task<List<ComparisonResult>> GetResultsBySessionIdPagedAsync(Guid sessionId, int skip, int take, CancellationToken ct = default)
        => await _context.ComparisonResults
            .Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.EntityType)
            .ThenBy(r => r.MatchKey)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public async Task AddSessionAsync(ComparisonSession session, CancellationToken ct = default)
    {
        await _context.ComparisonSessions.AddAsync(session, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateSessionAsync(ComparisonSession session, CancellationToken ct = default)
    {
        _context.ComparisonSessions.Update(session);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddResultsAsync(List<ComparisonResult> results, CancellationToken ct = default)
    {
        await _context.ComparisonResults.AddRangeAsync(results, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ScheduleSnapshot?> GetSnapshotByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ScheduleSnapshots.FindAsync(new object[] { id }, ct);

    public async Task<List<ScheduleSnapshot>> GetSnapshotsByProjectAsync(int projectId, CancellationToken ct = default)
        => await _context.ScheduleSnapshots
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CapturedAt)
            .ToListAsync(ct);

    public async Task AddSnapshotAsync(ScheduleSnapshot snapshot, CancellationToken ct = default)
    {
        await _context.ScheduleSnapshots.AddAsync(snapshot, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteSnapshotAsync(ScheduleSnapshot snapshot, CancellationToken ct = default)
    {
        _context.ScheduleSnapshots.Remove(snapshot);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ComparisonRule?> GetRuleByProjectAsync(int projectId, CancellationToken ct = default)
        => await _context.ComparisonRules
            .Where(r => r.ProjectId == projectId)
            .FirstOrDefaultAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
