using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class BudgetViewModel : ObservableObject
{
    private readonly ICostService _costService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private BudgetDto? _budget;

    [ObservableProperty]
    private decimal _contingencyAmount;

    [ObservableProperty]
    private decimal _contingencyPercent;

    [ObservableProperty]
    private bool _isManualOverride;

    [ObservableProperty]
    private decimal _manualTotal;

    [ObservableProperty]
    private bool _isLoading;

    public BudgetViewModel(ICostService costService, ICurrentProjectService currentProjectService)
    {
        _costService = costService;
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
            Budget = await _costService.GetBudgetAsync(projectId.Value);
            if (Budget != null)
            {
                ContingencyAmount = Budget.ContingencyAmount ?? 0;
                ContingencyPercent = Budget.ContingencyPercent ?? 0;
                IsManualOverride = Budget.IsManualOverride;
                ManualTotal = Budget.ManualTotalBudget ?? 0;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Budget == null) return;
        await _costService.UpdateBudgetAsync(Budget.Id, new UpdateBudgetRequest(
            ContingencyAmount, ContingencyPercent, IsManualOverride, ManualTotal));
        await LoadAsync();
    }
}
