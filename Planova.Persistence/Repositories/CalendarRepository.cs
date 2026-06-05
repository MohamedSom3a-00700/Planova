using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private readonly PlanovaDbContext _context;

    public CalendarRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<Calendar?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Calendar>> GetGlobalCalendarsAsync(CancellationToken ct = default)
    {
        return await _context.Calendars
            .Where(c => c.ProjectId == null)
            .ToListAsync(ct);
    }

    public async Task<List<Calendar>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Calendars
            .Where(c => c.ProjectId == null || c.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task<Calendar?> GetDefaultForProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Calendars
            .Where(c => (c.ProjectId == null || c.ProjectId == projectId) && c.IsDefault)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Calendar calendar, CancellationToken ct = default)
    {
        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Calendar calendar, CancellationToken ct = default)
    {
        _context.Calendars.Update(calendar);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Calendars.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.Calendars.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
