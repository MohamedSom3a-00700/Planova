using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ReportInstanceRepository : IReportInstanceRepository
{
    private readonly PlanovaDbContext _context;

    public ReportInstanceRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ReportInstance?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReportInstances
            .Include(i => i.Sections.OrderBy(s => s.OrderIndex))
            .Include(i => i.Exports)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<List<ReportInstance>> GetByProjectAsync(int projectId, ReportType? type = null, ReportStatus? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = _context.ReportInstances
            .Include(i => i.Exports)
            .Where(i => i.ProjectId == projectId);

        if (type.HasValue)
            query = query.Where(i => i.ReportType == type.Value);
        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);
        if (from.HasValue)
            query = query.Where(i => i.PeriodStart >= from.Value);
        if (to.HasValue)
            query = query.Where(i => i.PeriodEnd <= to.Value);

        return await query.OrderByDescending(i => i.GeneratedAt).ToListAsync(ct);
    }

    public async Task AddAsync(ReportInstance instance, CancellationToken ct = default)
    {
        await _context.ReportInstances.AddAsync(instance, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportInstance instance, CancellationToken ct = default)
    {
        _context.ReportInstances.Update(instance);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ReportInstance instance, CancellationToken ct = default)
    {
        _context.ReportInstances.Remove(instance);
        await _context.SaveChangesAsync(ct);
    }
}
