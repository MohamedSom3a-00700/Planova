namespace Planova.ScheduleComparison.Application.Models;

public class ScheduleCalendar
{
    public string CalendarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public double? HoursPerDay { get; set; }
    public List<CalendarDayEntry> Days { get; set; } = new();
}

public class CalendarDayEntry
{
    public DateOnly Date { get; set; }
    public bool IsWorkingDay { get; set; }
    public double? Hours { get; set; }
}
