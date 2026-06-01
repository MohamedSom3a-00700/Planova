using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly IUserProfileService _userProfileService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;

    public UserProfileViewModel(
        IUserProfileService userProfileService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ISettingsService settingsService)
    {
        _userProfileService = userProfileService;
        _themeService = themeService;
        _localizationService = localizationService;
        _settingsService = settingsService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string? _roleLabel;

    [ObservableProperty]
    private string? _organizationName;

    [ObservableProperty]
    private string _themePreference = "Dark";

    [ObservableProperty]
    private string _languagePreference = "en";

    [ObservableProperty]
    private string? _defaultWorkspace;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSaved;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var profile = await _userProfileService.GetProfileAsync();
            if (profile != null)
            {
                DisplayName = profile.DisplayName;
                RoleLabel = profile.RoleLabel;
                OrganizationName = profile.OrganizationName;
                ThemePreference = profile.ThemePreference;
                LanguagePreference = profile.LanguagePreference;
                DefaultWorkspace = profile.DefaultWorkspace;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profile: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        HasError = false;
        IsLoading = true;
        IsSaved = false;

        try
        {
            var dto = new UpdateUserProfileDto(
                DisplayName, RoleLabel, OrganizationName,
                ThemePreference, LanguagePreference, DefaultWorkspace);

            await _userProfileService.UpdateProfileAsync(dto);

            _themeService.SetTheme(ThemePreference);
            _localizationService.SetLanguage(LanguagePreference);
            _settingsService.Set("ThemePreference", ThemePreference);
            _settingsService.Set("LanguagePreference", LanguagePreference);
            await _settingsService.Save();

            IsSaved = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
