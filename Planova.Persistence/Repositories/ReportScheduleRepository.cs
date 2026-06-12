using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ReportScheduleRepository : IReportScheduleRepository
{
    private readonly PlanovaDbContext _context;

    public ReportScheduleRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportSchedule>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.ReportType)
            .ToListAsync(ct);
    }

    public async Task<ReportSchedule?> GetByProjectAndTypeAsync(int projectId, ReportType reportType, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.ReportType == reportType, ct);
    }

    public async Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReportSchedules.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<ReportSchedule>> GetDueSchedulesAsync(DateTime utcNow, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.IsActive && s.NextRunAt <= utcNow)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReportSchedule schedule, CancellationToken ct = default)
    {
        await _context.ReportSchedules.AddAsync(schedule, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportSchedule schedule, CancellationToken ct = default)
    {
        _context.ReportSchedules.Update(schedule);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ReportSchedule schedule, CancellationToken ct = default)
    {
        _context.ReportSchedules.Remove(schedule);
        await _context.SaveChangesAsync(ct);
    }
}
