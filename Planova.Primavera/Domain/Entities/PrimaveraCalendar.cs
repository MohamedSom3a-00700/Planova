using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Entities;

public class PrimaveraCalendar
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public string CalendarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsBaseCalendar { get; set; }
    public string? BaseCalendarId { get; set; }
    public string? WorkWeek { get; set; }
    public string? Exceptions { get; set; }
    public Guid? BaselineId { get; set; }
    public Guid ImportSessionId { get; set; }
    public PrimaveraSourceType SourceType { get; set; }
}
