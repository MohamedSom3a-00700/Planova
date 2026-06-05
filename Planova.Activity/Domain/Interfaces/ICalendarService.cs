using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Interfaces;

public interface ICalendarService
{
    Task<CalendarDto> CreateAsync(CreateCalendarRequest request, CancellationToken ct = default);
    Task<CalendarDto> UpdateAsync(UpdateCalendarRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<CalendarDto>> GetCalendarsAsync(int? projectId = null, CancellationToken ct = default);
    Task<CalendarDto?> GetDefaultAsync(int projectId, CancellationToken ct = default);
    Task<CalendarDayDto> SetDayStatusAsync(Guid calendarId, DateTime date, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task BulkSetDaysAsync(Guid calendarId, DateTime from, DateTime to, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task<List<CalendarDayDto>> GetDayRangeAsync(Guid calendarId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<DateTime> AddWorkingDaysAsync(DateTime startDate, int days, Guid? calendarId = null, int? projectId = null, CancellationToken ct = default);
    Task<int> CountWorkingDaysAsync(DateTime from, DateTime to, Guid? calendarId = null, int? projectId = null, CancellationToken ct = default);
}
