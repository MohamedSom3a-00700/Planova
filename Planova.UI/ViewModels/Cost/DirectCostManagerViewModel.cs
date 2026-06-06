using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class DirectCostManagerViewModel : ObservableObject
{
    private readonly IDirectCostService _directCostService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private ObservableCollection<DirectCostDto> _directCosts = new();

    [ObservableProperty]
    private DirectCostDto? _selectedDirectCost;

    [ObservableProperty]
    private bool _isLoading;

    public DirectCostManagerViewModel(
        IDirectCostService directCostService,
        ICurrentProjectService currentProjectService)
    {
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
            var costs = await _directCostService.GetByProjectIdAsync(projectId.Value);
            DirectCosts = new ObservableCollection<DirectCostDto>(costs);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddDirectCostAsync()
    {
        // TODO: Show add dialog
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task EditDirectCostAsync()
    {
        if (SelectedDirectCost == null) return;
        // TODO: Show edit dialog
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteDirectCostAsync()
    {
        if (SelectedDirectCost == null) return;
        await _directCostService.DeleteAsync(SelectedDirectCost.Id);
        DirectCosts.Remove(SelectedDirectCost);
    }
}
