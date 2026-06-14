using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraRelationshipsViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private List<PrimaveraRelationshipDto> _allRelationships = new();

    public PrimaveraRelationshipsViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraRelationshipDto> Relationships { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            _allRelationships = await _workspaceService.GetRelationshipsAsync(projectId, ct);
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
        Relationships.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allRelationships
            : _allRelationships.Where(r =>
                r.PredTaskId.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                r.SuccTaskId.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var item in filtered) Relationships.Add(item);
    }

    [RelayCommand]
    private async Task SaveRelationshipAsync(PrimaveraRelationshipDto? dto)
    {
        if (dto == null) return;
        await _workspaceService.UpdateRelationshipAsync(dto);
        var idx = _allRelationships.FindIndex(r => r.Id == dto.Id);
        if (idx >= 0) _allRelationships[idx] = dto;
    }
}
