using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class CalendarDayRepository : ICalendarDayRepository
{
    private readonly PlanovaDbContext _context;

    public CalendarDayRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<CalendarDay?> GetAsync(Guid calendarId, DateTime date, CancellationToken ct = default)
    {
        return await _context.CalendarDays
            .FirstOrDefaultAsync(d => d.CalendarId == calendarId && d.Date == date.Date, ct);
    }

    public async Task<List<CalendarDay>> GetRangeAsync(Guid calendarId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.CalendarDays
            .Where(d => d.CalendarId == calendarId && d.Date >= from.Date && d.Date <= to.Date)
            .OrderBy(d => d.Date)
            .ToListAsync(ct);
    }

    public async Task SetWorkingStatusAsync(Guid calendarId, DateTime date, CalendarDayStatus status, string? label = null, CancellationToken ct = default)
    {
        var existing = await _context.CalendarDays
            .FirstOrDefaultAsync(d => d.CalendarId == calendarId && d.Date == date.Date, ct);

        if (existing is not null)
        {
            existing.Status = status;
            existing.Label = label;
            _context.CalendarDays.Update(existing);
        }
        else
        {
            _context.CalendarDays.Add(new CalendarDay
            {
                Id = Guid.NewGuid(),
                CalendarId = calendarId,
                Date = date.Date,
                Status = status,
                Label = label,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task BulkSetRangeAsync(Guid calendarId, DateTime from, DateTime to, CalendarDayStatus status, string? label = null, CancellationToken ct = default)
    {
        var existing = await _context.CalendarDays
            .Where(d => d.CalendarId == calendarId && d.Date >= from.Date && d.Date <= to.Date)
            .ToListAsync(ct);

        var existingDates = existing.Select(d => d.Date).ToHashSet();

        foreach (var day in existing)
        {
            day.Status = status;
            day.Label = label;
        }

        var current = from.Date;
        while (current <= to.Date)
        {
            if (!existingDates.Contains(current))
            {
                _context.CalendarDays.Add(new CalendarDay
                {
                    Id = Guid.NewGuid(),
                    CalendarId = calendarId,
                    Date = current,
                    Status = status,
                    Label = label,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            current = current.AddDays(1);
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CalendarDays.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.CalendarDays.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
