using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Entities;

public class CalendarDay
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public DateTime Date { get; set; }
    public CalendarDayStatus Status { get; set; }
    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Calendar Calendar { get; set; } = null!;
}
