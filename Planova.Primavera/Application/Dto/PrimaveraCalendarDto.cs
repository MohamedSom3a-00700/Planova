namespace Planova.Primavera.Application.Dto;

public class PrimaveraCalendarDto
{
    public Guid Id { get; set; }
    public string CalendarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsBaseCalendar { get; set; }
    public string? BaseCalendarId { get; set; }
    public string SourceType { get; set; } = string.Empty;
}
