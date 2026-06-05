using Planova.Activity.Domain.Enums;

namespace Planova.Activity.Domain.Entities;

public class Calendar
{
    public Guid Id { get; set; }
    public int? ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CalendarType Type { get; set; }
    public decimal HoursPerDay { get; set; } = 8;
    public int DaysPerWeek { get; set; } = 5;
    public bool Monday { get; set; } = true;
    public bool Tuesday { get; set; } = true;
    public bool Wednesday { get; set; } = true;
    public bool Thursday { get; set; } = true;
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<CalendarDay> Days { get; set; } = new List<CalendarDay>();

    public static Calendar GetDefault() => new()
    {
        Id = Guid.Empty,
        Name = "Standard 5-Day Work Week",
        Type = CalendarType.Global,
        HoursPerDay = 8,
        DaysPerWeek = 5,
        Monday = true,
        Tuesday = true,
        Wednesday = true,
        Thursday = true,
        Friday = false,
        Saturday = false,
        Sunday = true,
        IsDefault = true
    };
}
