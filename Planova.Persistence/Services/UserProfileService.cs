using Planova.Application.Dto;
using Planova.Application.Repositories;
using Planova.Application.Services;

namespace Planova.Persistence.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;

    public UserProfileService(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserProfileDto?> GetProfileAsync(CancellationToken ct = default)
    {
        var prefs = await _repository.GetAsync(ct);
        if (prefs == null) return null;

        return MapToDto(prefs);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(UpdateUserProfileDto dto, CancellationToken ct = default)
    {
        var prefs = await _repository.GetAsync(ct) ?? new Domain.Entities.UserPreferences();

        prefs.DisplayName = dto.DisplayName;
        prefs.RoleLabel = dto.RoleLabel;
        prefs.OrganizationName = dto.OrganizationName;
        prefs.ThemePreference = dto.ThemePreference;
        prefs.LanguagePreference = dto.LanguagePreference;
        prefs.DefaultWorkspace = dto.DefaultWorkspace;
        prefs.UpdatedAt = DateTime.UtcNow;

        var saved = await _repository.SaveAsync(prefs, ct);
        return MapToDto(saved);
    }

    private static UserProfileDto MapToDto(Domain.Entities.UserPreferences prefs)
    {
        return new UserProfileDto(
            prefs.Id,
            prefs.DisplayName,
            prefs.RoleLabel,
            prefs.OrganizationName,
            prefs.ThemePreference,
            prefs.LanguagePreference,
            prefs.DefaultWorkspace
        );
    }
}
