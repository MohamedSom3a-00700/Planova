using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraBaselinesViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;

    public PrimaveraBaselinesViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraBaselineDto> Baselines { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            Baselines.Clear();
            var items = await _workspaceService.GetBaselinesAsync(projectId, ct);
            foreach (var item in items)
                Baselines.Add(item);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
