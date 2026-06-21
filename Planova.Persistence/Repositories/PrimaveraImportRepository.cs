using Microsoft.EntityFrameworkCore;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class PrimaveraImportRepository : IPrimaveraImportRepository
{
    private readonly PlanovaDbContext _context;

    public PrimaveraImportRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<XerImportSession> CreateSessionAsync(XerImportSession session, CancellationToken ct = default)
    {
        _context.Set<XerImportSession>().Add(session);
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<XerImportSession?> GetSessionByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<XerImportSession>().FindAsync(new object[] { id }, ct);
    }

    public async Task<List<XerImportSession>> GetSessionsAsync(CancellationToken ct = default)
    {
        return await _context.Set<XerImportSession>()
            .OrderByDescending(s => s.ImportedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateSessionAsync(XerImportSession session, CancellationToken ct = default)
    {
        _context.Set<XerImportSession>().Update(session);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SaveRawTablesAsync(IEnumerable<XerRawTable> tables, CancellationToken ct = default)
    {
        _context.Set<XerRawTable>().AddRange(tables);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PrimaveraProject?> GetProjectByXerIdAsync(string projectId, CancellationToken ct = default)
    {
        return await _context.Set<PrimaveraProject>()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, ct);
    }

    public async Task<bool> HasExistingProjectByXerIdAsync(string xerProjectId, CancellationToken ct = default)
    {
        return await _context.Set<XerImportSession>()
            .AnyAsync(s => s.ProjectCode == xerProjectId && s.Status == Primavera.Domain.Enums.PrimaveraImportStatus.Committed, ct);
    }

    public async Task DeleteAllXerDataAsync(CancellationToken ct = default)
    {
        await _context.Set<PrimaveraRepairAction>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraValidationIssue>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraValidationRule>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraUdf>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraBaseline>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraCode>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraCalendar>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraResourceAssignment>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraRelationship>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraActivity>().ExecuteDeleteAsync(ct);
        await _context.Set<PrimaveraProject>().ExecuteDeleteAsync(ct);
        await _context.Set<XerRawTable>().ExecuteDeleteAsync(ct);
        await _context.Set<XerExportProfile>().ExecuteDeleteAsync(ct);
        await _context.Set<XerImportSession>().ExecuteDeleteAsync(ct);
    }

    public async Task<List<XerImportSession>> GetSessionsByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<XerImportSession>()
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.ImportedAt)
            .ToListAsync(ct);
    }

    public async Task PersistImportDataAsync(int projectId, Guid sessionId,
        List<PrimaveraActivity> activities, List<PrimaveraRelationship> relationships,
        List<PrimaveraResourceAssignment> resourceAssignments, List<PrimaveraCalendar> calendars,
        List<PrimaveraCode> codes, List<PrimaveraBaseline> baselines, List<PrimaveraUdf> udfs,
        CancellationToken ct = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                _context.Set<PrimaveraActivity>().RemoveRange(
                    await _context.Set<PrimaveraActivity>().Where(a => a.ProjectId == projectId).ToListAsync(ct));
                _context.Set<PrimaveraRelationship>().RemoveRange(
                    await _context.Set<PrimaveraRelationship>().Where(r => r.ProjectId == projectId).ToListAsync(ct));
                _context.Set<PrimaveraResourceAssignment>().RemoveRange(
                    await _context.Set<PrimaveraResourceAssignment>().Where(r => r.ProjectId == projectId).ToListAsync(ct));
                _context.Set<PrimaveraCalendar>().RemoveRange(
                    await _context.Set<PrimaveraCalendar>().Where(c => c.ProjectId == projectId).ToListAsync(ct));
                _context.Set<PrimaveraCode>().RemoveRange(
                    await _context.Set<PrimaveraCode>().Where(c => c.ProjectId == projectId).ToListAsync(ct));
                _context.Set<PrimaveraBaseline>().RemoveRange(
                    await _context.Set<PrimaveraBaseline>().Where(b => b.ProjectId == projectId).ToListAsync(ct));
                _context.Set<PrimaveraUdf>().RemoveRange(
                    await _context.Set<PrimaveraUdf>().Where(u => u.ProjectId == projectId).ToListAsync(ct));
                await _context.SaveChangesAsync(ct);

                _context.Set<PrimaveraActivity>().AddRange(activities);
                _context.Set<PrimaveraRelationship>().AddRange(relationships);
                _context.Set<PrimaveraResourceAssignment>().AddRange(resourceAssignments);
                _context.Set<PrimaveraCalendar>().AddRange(calendars);
                _context.Set<PrimaveraCode>().AddRange(codes);
                _context.Set<PrimaveraBaseline>().AddRange(baselines);
                _context.Set<PrimaveraUdf>().AddRange(udfs);
                await _context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}
