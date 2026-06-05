using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Excel.Models;
using Planova.Excel.Services;

namespace Planova.UI.ViewModels.Excel;

public partial class MappingProfilesViewModel : ObservableObject
{
    private readonly IMappingProfileService _profileService;

    public MappingProfilesViewModel(IMappingProfileService profileService)
    {
        _profileService = profileService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _selectedEntityType = string.Empty;

    [ObservableProperty]
    private MappingProfile? _selectedProfile;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _cloneName = string.Empty;

    public ObservableCollection<string> EntityTypes { get; } = new()
    {
        "Project", "Activity", "Resource", "Cost", "Risk"
    };

    public ObservableCollection<MappingProfile> Profiles { get; } = new();

    [RelayCommand]
    private async Task LoadProfilesAsync(string? entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType)) return;

        IsLoading = true;
        HasError = false;

        try
        {
            SelectedEntityType = entityType;
            var profiles = await _profileService.GetAllAsync(entityType, CancellationToken.None);
            Profiles.Clear();
            foreach (var p in profiles)
                Profiles.Add(p);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profiles: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectProfile(MappingProfile? profile)
    {
        SelectedProfile = profile;
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(SelectedEntityType))
            return;

        IsLoading = true;
        HasError = false;

        try
        {
            var profile = await _profileService.CreateAsync(
                EditName, SelectedEntityType, new Dictionary<string, string>(), CancellationToken.None);

            Profiles.Insert(0, profile);
            SelectedProfile = profile;
            EditName = string.Empty;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create profile: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateProfileAsync()
    {
        if (SelectedProfile is null || string.IsNullOrWhiteSpace(EditName))
            return;

        IsLoading = true;
        HasError = false;

        try
        {
            var updated = await _profileService.UpdateAsync(
                SelectedProfile.Id, EditName, SelectedProfile.ColumnMappings, CancellationToken.None);

            var index = Profiles.IndexOf(SelectedProfile);
            if (index >= 0)
            {
                Profiles[index] = updated;
                SelectedProfile = updated;
            }

            IsEditing = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to update profile: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            await _profileService.DeleteAsync(SelectedProfile.Id, CancellationToken.None);
            Profiles.Remove(SelectedProfile);
            SelectedProfile = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete profile: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CloneProfileAsync()
    {
        if (SelectedProfile is null || string.IsNullOrWhiteSpace(CloneName))
            return;

        IsLoading = true;
        HasError = false;

        try
        {
            var clone = await _profileService.CloneAsync(SelectedProfile.Id, CloneName, CancellationToken.None);
            Profiles.Insert(0, clone);
            SelectedProfile = clone;
            CloneName = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to clone profile: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void StartEditing()
    {
        if (SelectedProfile is null) return;
        EditName = SelectedProfile.Name;
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEditing()
    {
        IsEditing = false;
        EditName = string.Empty;
    }
}
