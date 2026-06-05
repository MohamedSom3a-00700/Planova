using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceRateManagerViewModel : ObservableObject
{
    private readonly IResourceRateService _rateService;

    [ObservableProperty]
    private Guid _resourceId;

    [ObservableProperty]
    private ObservableCollection<ResourceRateDto> _rates = new();

    [ObservableProperty]
    private ResourceRateDto? _selectedRate;

    [ObservableProperty]
    private decimal _newRateValue;

    [ObservableProperty]
    private DateTime _newEffectiveDate = DateTime.Today;

    [ObservableProperty]
    private string _newCurrency = "USD";

    [ObservableProperty]
    private string _newUnitOfMeasure = "hr";

    public ResourceRateManagerViewModel(IResourceRateService rateService)
    {
        _rateService = rateService;
    }

    [RelayCommand]
    private async Task LoadRateHistory(Guid resourceId)
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
        await LoadRateHistory(ResourceId);
    }

    [RelayCommand]
    private async Task RemoveRate(ResourceRateDto? rate)
    {
        if (rate is null) return;
        await _rateService.DeleteRateAsync(rate.Id);
        await LoadRateHistory(ResourceId);
    }

    [RelayCommand]
    private async Task BulkRateUpdate()
    {
        if (ResourceId == Guid.Empty) return;

        await _rateService.BulkUpdateRatesAsync([ResourceId], NewRateValue, DateTime.Today, "USD", "hr");
        await LoadRateHistory(ResourceId);
    }
}
