using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceEditorViewModel : ObservableObject
{
    private readonly IResourceService _resourceService;

    [ObservableProperty]
    private ResourceDto? _resource;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _showLabourFields;

    [ObservableProperty]
    private bool _showEquipmentFields;

    [ObservableProperty]
    private bool _showMaterialFields;

    [ObservableProperty]
    private bool _showSubcontractorFields;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ResourceType _resourceType;

    [ObservableProperty]
    private ResourceScope _scope;

    [ObservableProperty]
    private string _unitOfMeasure = "hr";

    [ObservableProperty]
    private decimal _defaultRate;

    [ObservableProperty]
    private string _currency = "USD";

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _warningMessage;

    public event Action? OnSaved;

    public ObservableCollection<ResourceRateDto> Rates { get; } = new();

    public ResourceEditorViewModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    partial void OnResourceTypeChanged(ResourceType value)
    {
        ShowLabourFields = value == ResourceType.Labour;
        ShowEquipmentFields = value == ResourceType.Equipment;
        ShowMaterialFields = value == ResourceType.Material;
        ShowSubcontractorFields = value == ResourceType.Subcontractor;
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            if (IsNew)
            {
                var request = new CreateResourceRequest
                {
                    Name = Name,
                    ResourceType = ResourceType,
                    Scope = Scope,
                    DefaultRate = DefaultRate,
                    UnitOfMeasure = UnitOfMeasure,
                    Currency = Currency,
                    Description = Description
                };
                await _resourceService.CreateAsync(request);
            }
            else if (Resource is not null)
            {
                var request = new UpdateResourceRequest
                {
                    Id = Resource.Id,
                    Name = Name,
                    DefaultRate = DefaultRate,
                    UnitOfMeasure = UnitOfMeasure,
                    Currency = Currency,
                    Description = Description
                };
                await _resourceService.UpdateAsync(request);
            }

            WarningMessage = null;
            OnSaved?.Invoke();
        }
        catch (Exception ex)
        {
            WarningMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Task.CompletedTask;
    }

    public void InitializeForCreate(ResourceType type)
    {
        IsNew = true;
        Title = "Create Resource";
        ResourceType = type;
    }

    public void InitializeForEdit(ResourceDto dto)
    {
        IsNew = false;
        Title = "Edit Resource";
        Resource = dto;
        Name = dto.Name;
        ResourceType = dto.ResourceType;
        Scope = dto.Scope;
        DefaultRate = dto.DefaultRate;
        UnitOfMeasure = dto.UnitOfMeasure;
        Currency = dto.Currency;
        Description = dto.Description;
    }
}
