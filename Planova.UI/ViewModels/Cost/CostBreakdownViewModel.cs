using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class CostBreakdownViewModel : ObservableObject
{
    private readonly ICostService _costService;
    private readonly IDirectCostService _directCostService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private ObservableCollection<CostBreakdownDto> _treeItems = new();

    [ObservableProperty]
    private CostBreakdownDto? _selectedItem;

    [ObservableProperty]
    private bool _isLoading;

    public CostBreakdownViewModel(
        ICostService costService,
        IDirectCostService directCostService,
        ICurrentProjectService currentProjectService)
    {
        _costService = costService;
        _directCostService = directCostService;
        _currentProjectService = currentProjectService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            var breakdown = await _costService.GetCostBreakdownAsync(projectId.Value);
            TreeItems = new ObservableCollection<CostBreakdownDto>(breakdown.Children ?? new List<CostBreakdownDto>());
        }
        finally
        {
            IsLoading = false;
        }
    }
}
