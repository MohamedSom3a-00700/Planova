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

    public async Task<bool> HasDuplicateFileAsync(string fileHash, CancellationToken ct = default)
    {
        return await _context.Set<XerImportSession>()
            .AnyAsync(s => s.SourceFileHash == fileHash && s.Status == Primavera.Domain.Enums.PrimaveraImportStatus.Committed, ct);
    }
}
