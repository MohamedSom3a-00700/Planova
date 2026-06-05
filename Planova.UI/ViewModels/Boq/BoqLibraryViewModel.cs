using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqLibraryViewModel : ObservableObject
{
    private readonly LibraryService _libraryService;
    private readonly IBoqSession _session;

    public BoqLibraryViewModel(LibraryService libraryService, IBoqSession session)
    {
        _libraryService = libraryService;
        _session = session;
        _session.BoqChanged += OnBoqChanged;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasBoq;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private LibraryDto? _selectedLibrary;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private LibraryType _newLibraryType = LibraryType.UserDefined;

    public ObservableCollection<LibraryDto> Libraries { get; } = new();
    public List<LibraryType> LibraryTypeOptions { get; } = [LibraryType.System, LibraryType.UserDefined];

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        HasBoq = true;
        await LoadLibrariesAsync();
    }

    [RelayCommand]
    private async Task LoadLibrariesAsync()
    {
        try
        {
            IsLoading = true;
            var libraries = await _libraryService.GetAllAsync(CancellationToken.None);
            Libraries.Clear();
            foreach (var lib in libraries)
            {
                Libraries.Add(lib);
            }
            StatusMessage = $"Loaded {libraries.Count} libraries";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddLibraryAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;
        try
        {
            IsLoading = true;
            var created = await _libraryService.CreateAsync(NewName, NewDescription, NewLibraryType, ct);
            StatusMessage = $"Added: {created.Name}";
            ClearNewForm();
            await LoadLibrariesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Add error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLibraryAsync(CancellationToken ct)
    {
        if (SelectedLibrary == null) return;
        try
        {
            IsLoading = true;
            await _libraryService.DeleteAsync(SelectedLibrary.Id, ct);
            StatusMessage = "Deleted";
            SelectedLibrary = null;
            await LoadLibrariesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearNewForm()
    {
        NewName = string.Empty;
        NewDescription = string.Empty;
        NewLibraryType = LibraryType.UserDefined;
    }
}
