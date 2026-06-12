using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ReportTemplateRepository : IReportTemplateRepository
{
    private readonly PlanovaDbContext _context;

    public ReportTemplateRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportTemplate>> GetByProjectAsync(int projectId, ReportType? reportType = null, CancellationToken ct = default)
    {
        var query = _context.ReportTemplates.Where(t => t.ProjectId == projectId);
        if (reportType.HasValue)
            query = query.Where(t => t.ReportType == reportType.Value);
        return await query.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<List<ReportTemplate>> GetGlobalTemplatesAsync(ReportType? reportType = null, CancellationToken ct = default)
    {
        var query = _context.ReportTemplates.Where(t => t.ProjectId == null);
        if (reportType.HasValue)
            query = query.Where(t => t.ReportType == reportType.Value);
        return await query.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<ReportTemplate?> GetDefaultForProjectAsync(int projectId, ReportType reportType, CancellationToken ct = default)
    {
        return await _context.ReportTemplates
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.ReportType == reportType && t.IsDefault, ct);
    }

    public async Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReportTemplates.FindAsync(new object[] { id }, ct);
    }

    public async Task AddAsync(ReportTemplate template, CancellationToken ct = default)
    {
        await _context.ReportTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportTemplate template, CancellationToken ct = default)
    {
        _context.ReportTemplates.Update(template);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ReportTemplate template, CancellationToken ct = default)
    {
        _context.ReportTemplates.Remove(template);
        await _context.SaveChangesAsync(ct);
    }
}
