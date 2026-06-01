namespace Planova.Domain.Entities;

public class UserPreferences
{
    public int Id { get; set; }
    public string ThemePreference { get; set; } = "Dark";
    public string LanguagePreference { get; set; } = "en";
    public int? WindowX { get; set; }
    public int? WindowY { get; set; }
    public int? WindowWidth { get; set; }
    public int? WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
