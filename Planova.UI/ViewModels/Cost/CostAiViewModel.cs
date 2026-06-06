using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class CostAiViewModel : ObservableObject
{
    private readonly ICostAiService _costAiService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private AiSuggestionDto? _suggestion;

    [ObservableProperty]
    private List<CostAnomalyDto> _anomalies = new();

    [ObservableProperty]
    private AiForecastDto? _forecast;

    [ObservableProperty]
    private string _narrative = string.Empty;

    [ObservableProperty]
    private bool _isAiAvailable;

    [ObservableProperty]
    private bool _isLoading;

    public CostAiViewModel(
        ICostAiService costAiService,
        ICurrentProjectService currentProjectService)
    {
        _costAiService = costAiService;
        _currentProjectService = currentProjectService;
        IsAiAvailable = costAiService.IsAvailable;
    }

    [RelayCommand]
    private async Task EstimateCostAsync()
    {
        IsLoading = true;
        try
        {
            Suggestion = await _costAiService.EstimateCostAsync(Guid.Empty);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DetectAnomaliesAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            Anomalies = await _costAiService.DetectAnomaliesAsync(projectId.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ForecastAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            Forecast = await _costAiService.ForecastEacAsync(projectId.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GenerateNarrativeAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            Narrative = await _costAiService.GenerateNarrativeAsync(projectId.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
