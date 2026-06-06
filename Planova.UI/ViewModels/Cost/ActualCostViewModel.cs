using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class ActualCostViewModel : ObservableObject
{
    private readonly IActualCostService _actualCostService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private ObservableCollection<ActivityVarianceDto> _variances = new();

    [ObservableProperty]
    private ActivityVarianceDto? _selectedVariance;

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private bool _isLoading;

    public ActualCostViewModel(
        IActualCostService actualCostService,
        ICurrentProjectService currentProjectService)
    {
        _actualCostService = actualCostService;
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
            var items = await _actualCostService.GetVarianceByProjectAsync(projectId.Value);
            Variances = new ObservableCollection<ActivityVarianceDto>(items);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        // TODO: File picker dialog for Excel import
        await Task.CompletedTask;
    }
}
