using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceRateManagerViewModel : ObservableObject
{
    private readonly IResourceRateService _rateService;
    private readonly IResourceService _resourceService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private Guid _resourceId;

    [ObservableProperty]
    private ObservableCollection<ResourceRateDto> _rates = new();

    [ObservableProperty]
    private ResourceRateDto? _selectedRate;

    [ObservableProperty]
    private ObservableCollection<ResourceDto> _availableResources = new();

    [ObservableProperty]
    private ResourceDto? _selectedResource;

    [ObservableProperty]
    private decimal _newRateValue;

    [ObservableProperty]
    private DateTime _newEffectiveDate = DateTime.Today;

    [ObservableProperty]
    private string _newCurrency = "USD";

    [ObservableProperty]
    private string _newUnitOfMeasure = "hr";

    public ResourceRateManagerViewModel(
        IResourceRateService rateService,
        IResourceService resourceService,
        ICurrentProjectService currentProjectService)
    {
        _rateService = rateService;
        _resourceService = resourceService;
        _currentProjectService = currentProjectService;
        _currentProjectService.CurrentProjectChanged += OnProjectChanged;
        _ = LoadResourcesAsync();
    }

    partial void OnSelectedResourceChanged(ResourceDto? value)
    {
        if (value is not null)
            _ = LoadRateHistoryAsync(value.Id);
    }

    private void OnProjectChanged(object? sender, ProjectContext? project)
    {
        _ = LoadResourcesAsync();
    }

    [RelayCommand]
    private async Task LoadResourcesAsync()
    {
        var filter = new ResourceFilter
        {
            ProjectId = _currentProjectService.CurrentProject?.Id
        };
        var resources = await _resourceService.SearchAsync(filter);
        AvailableResources = new ObservableCollection<ResourceDto>(resources);
    }

    [RelayCommand]
    private async Task LoadRateHistoryAsync(Guid resourceId)
    {
        ResourceId = resourceId;
        var rates = await _rateService.GetRateHistoryAsync(resourceId);
        Rates = new ObservableCollection<ResourceRateDto>(rates);
    }

    [RelayCommand]
    private async Task AddRate()
    {
        if (ResourceId == Guid.Empty) return;

        var request = new CreateRateRequest
        {
            ResourceId = ResourceId,
            EffectiveDate = NewEffectiveDate,
            Rate = NewRateValue,
            Currency = NewCurrency,
            UnitOfMeasure = NewUnitOfMeasure
        };

        await _rateService.AddRateAsync(request);
        NewRateValue = 0;
        NewEffectiveDate = DateTime.Today;
        await LoadRateHistoryAsync(ResourceId);
    }

    [RelayCommand]
    private async Task RemoveRate(ResourceRateDto? rate)
    {
        if (rate is null) return;
        await _rateService.DeleteRateAsync(rate.Id);
        await LoadRateHistoryAsync(ResourceId);
    }

    [RelayCommand]
    private async Task BulkRateUpdate()
    {
        if (ResourceId == Guid.Empty) return;

        await _rateService.BulkUpdateRatesAsync([ResourceId], NewRateValue, DateTime.Today, "USD", "hr");
        await LoadRateHistoryAsync(ResourceId);
    }
}
