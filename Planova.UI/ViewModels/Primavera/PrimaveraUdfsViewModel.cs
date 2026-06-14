using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraUdfsViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private List<PrimaveraUdfDto> _allUdfs = new();

    public PrimaveraUdfsViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraUdfDto> Udfs { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            _allUdfs = await _workspaceService.GetUdfsAsync(projectId, ct);
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
        Udfs.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allUdfs
            : _allUdfs.Where(u =>
                u.FieldName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                u.TableName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var item in filtered) Udfs.Add(item);
    }

    [RelayCommand]
    private async Task SaveUdfAsync(PrimaveraUdfDto? dto)
    {
        if (dto == null) return;
        await _workspaceService.UpdateUdfAsync(dto);
        var idx = _allUdfs.FindIndex(u => u.Id == dto.Id);
        if (idx >= 0) _allUdfs[idx] = dto;
    }
}
