using Microsoft.EntityFrameworkCore;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class PrimaveraExportRepository : IPrimaveraExportRepository
{
    private readonly PlanovaDbContext _context;

    public PrimaveraExportRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<XerRawTable>> GetRawTablesAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Set<XerRawTable>()
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(ct);
    }
}
