using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraResourcesViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private List<PrimaveraResourceAssignmentDto> _allAssignments = new();

    public PrimaveraResourcesViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraResourceAssignmentDto> ResourceAssignments { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            _allAssignments = await _workspaceService.GetResourceAssignmentsAsync(projectId, ct);
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        ResourceAssignments.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allAssignments
            : _allAssignments.Where(r =>
                r.TaskId.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                r.ResourceId.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var item in filtered) ResourceAssignments.Add(item);
    }

    [RelayCommand]
    private async Task SaveResourceAssignmentAsync(PrimaveraResourceAssignmentDto? dto)
    {
        if (dto == null) return;
        await _workspaceService.UpdateResourceAssignmentAsync(dto);
        var idx = _allAssignments.FindIndex(r => r.Id == dto.Id);
        if (idx >= 0) _allAssignments[idx] = dto;
    }
}
