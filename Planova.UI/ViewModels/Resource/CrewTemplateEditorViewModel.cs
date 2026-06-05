using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;

namespace Planova.UI.ViewModels.Resource;

public partial class CrewTemplateEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private CrewDto? _crew;

    [ObservableProperty]
    private decimal _blendedRate;

    [ObservableProperty]
    private ObservableCollection<CrewResourceDto> _resources = new();

    [RelayCommand]
    private async Task AddResource()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RemoveResource(CrewResourceDto? resource)
    {
        await Task.CompletedTask;
    }

    public async Task UpdateQuantity(CrewResourceDto? resource, decimal quantity)
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Save()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Task.CompletedTask;
    }
}
