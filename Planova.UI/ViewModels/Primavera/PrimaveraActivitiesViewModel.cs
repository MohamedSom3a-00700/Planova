using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraActivitiesViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private List<PrimaveraActivityDto> _allActivities = new();

    public PrimaveraActivitiesViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraActivityDto> Activities { get; } = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasChanges;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            _allActivities = await _workspaceService.GetActivitiesAsync(projectId, ct);
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Activities.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allActivities
            : _allActivities.Where(a =>
                a.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                a.TaskId.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                a.Status.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var item in filtered)
            Activities.Add(item);
    }

    [RelayCommand]
    private async Task SaveActivityAsync(PrimaveraActivityDto? dto)
    {
        if (dto == null) return;
        var success = await _workspaceService.UpdateActivityAsync(dto);
        if (success)
        {
            HasChanges = true;
            var idx = _allActivities.FindIndex(a => a.Id == dto.Id);
            if (idx >= 0) _allActivities[idx] = dto;
        }
    }
}
