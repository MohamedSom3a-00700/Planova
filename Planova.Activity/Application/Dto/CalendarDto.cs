namespace Planova.Activity.Application.Dto;

public record CalendarDto
{
    public Guid Id { get; init; }
    public int? ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal HoursPerDay { get; init; }
    public int DaysPerWeek { get; init; }
    public bool Monday { get; init; }
    public bool Tuesday { get; init; }
    public bool Wednesday { get; init; }
    public bool Thursday { get; init; }
    public bool Friday { get; init; }
    public bool Saturday { get; init; }
    public bool Sunday { get; init; }
    public bool IsDefault { get; init; }
    public string? Description { get; init; }
}

public record CreateCalendarRequest
{
    public int? ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal HoursPerDay { get; init; } = 8;
    public int DaysPerWeek { get; init; } = 5;
    public bool Monday { get; init; } = true;
    public bool Tuesday { get; init; } = true;
    public bool Wednesday { get; init; } = true;
    public bool Thursday { get; init; } = true;
    public bool Friday { get; init; }
    public bool Saturday { get; init; }
    public bool Sunday { get; init; }
    public bool IsDefault { get; init; }
    public string? Description { get; init; }
}

public record UpdateCalendarRequest
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public decimal? HoursPerDay { get; init; }
    public int? DaysPerWeek { get; init; }
    public bool? Monday { get; init; }
    public bool? Tuesday { get; init; }
    public bool? Wednesday { get; init; }
    public bool? Thursday { get; init; }
    public bool? Friday { get; init; }
    public bool? Saturday { get; init; }
    public bool? Sunday { get; init; }
    public bool? IsDefault { get; init; }
    public string? Description { get; init; }
}

public record CalendarDayDto
{
    public Guid Id { get; init; }
    public Guid CalendarId { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Label { get; init; }
}
