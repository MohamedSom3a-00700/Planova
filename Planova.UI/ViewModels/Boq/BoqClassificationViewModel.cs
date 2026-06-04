using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqClassificationViewModel : ObservableObject
{
    private readonly ClassificationService _classificationService;
    private readonly IBoqSession _session;

    public BoqClassificationViewModel(ClassificationService classificationService, IBoqSession session)
    {
        _classificationService = classificationService;
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
    private ClassificationDto? _selectedClassification;

    [ObservableProperty]
    private string _newCode = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private ClassificationScope _newScope = ClassificationScope.Project;

    public ObservableCollection<ClassificationDto> Classifications { get; } = new();
    public List<ClassificationScope> ScopeOptions { get; } = [ClassificationScope.Project, ClassificationScope.Global];

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        HasBoq = true;
        await LoadClassificationsAsync();
    }

    [RelayCommand]
    private async Task LoadClassificationsAsync()
    {
        try
        {
            IsLoading = true;
            var tree = await _classificationService.GetTreeAsync(_session.CurrentProjectId, CancellationToken.None);
            Classifications.Clear();
            foreach (var item in tree)
            {
                Classifications.Add(item);
            }
            StatusMessage = $"Loaded {tree.Count} root classifications";
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
    private async Task AddClassificationAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(NewCode) || string.IsNullOrWhiteSpace(NewName)) return;
        try
        {
            IsLoading = true;
            var created = await _classificationService.CreateAsync(
                NewCode, NewName, NewDescription,
                NewScope, _session.CurrentProjectId,
                SelectedClassification?.Id, ct);
            StatusMessage = $"Added: {created.Code}";
            ClearNewForm();
            await LoadClassificationsAsync();
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
    private async Task DeleteClassificationAsync(CancellationToken ct)
    {
        if (SelectedClassification == null) return;
        try
        {
            IsLoading = true;
            await _classificationService.DeleteAsync(SelectedClassification.Id, ct);
            StatusMessage = "Deleted";
            SelectedClassification = null;
            await LoadClassificationsAsync();
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
        NewCode = string.Empty;
        NewName = string.Empty;
        NewDescription = string.Empty;
        NewScope = ClassificationScope.Project;
    }
}
