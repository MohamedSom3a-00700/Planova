namespace Planova.Application.Dto;

public record UserProfileDto(
    int Id,
    string DisplayName,
    string? RoleLabel,
    string? OrganizationName,
    string ThemePreference,
    string LanguagePreference,
    string? DefaultWorkspace
);

public record UpdateUserProfileDto(
    string DisplayName,
    string? RoleLabel,
    string? OrganizationName,
    string ThemePreference,
    string LanguagePreference,
    string? DefaultWorkspace
);
