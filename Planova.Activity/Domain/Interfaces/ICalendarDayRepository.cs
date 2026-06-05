using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Interfaces;

public interface ICalendarDayRepository
{
    Task<CalendarDay?> GetAsync(Guid calendarId, DateTime date, CancellationToken ct = default);
    Task<List<CalendarDay>> GetRangeAsync(Guid calendarId, DateTime from, DateTime to, CancellationToken ct = default);
    Task SetWorkingStatusAsync(Guid calendarId, DateTime date, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task BulkSetRangeAsync(Guid calendarId, DateTime from, DateTime to, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
