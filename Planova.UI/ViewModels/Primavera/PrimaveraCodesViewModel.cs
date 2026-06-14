using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraCodesViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private List<PrimaveraCodeDto> _allCodes = new();

    public PrimaveraCodesViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraCodeDto> Codes { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            _allCodes = await _workspaceService.GetCodesAsync(projectId, ct);
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
        Codes.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allCodes
            : _allCodes.Where(c =>
                c.CodeName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                c.CodeType.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                c.CodeValue.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var item in filtered) Codes.Add(item);
    }

    [RelayCommand]
    private async Task SaveCodeAsync(PrimaveraCodeDto? dto)
    {
        if (dto == null) return;
        await _workspaceService.UpdateCodeAsync(dto);
        var idx = _allCodes.FindIndex(c => c.Id == dto.Id);
        if (idx >= 0) _allCodes[idx] = dto;
    }
}
