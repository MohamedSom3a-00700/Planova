using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class CrewTemplateManagerViewModel : ObservableObject
{
    private readonly ICrewService _crewService;

    [ObservableProperty]
    private ObservableCollection<CrewDto> _crews = new();

    [ObservableProperty]
    private CrewDto? _selectedCrew;

    [ObservableProperty]
    private string _newCrewName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CrewTemplateManagerViewModel(ICrewService crewService)
    {
        _crewService = crewService;
    }

    [RelayCommand]
    private async Task LoadCrews()
    {
        var crews = await _crewService.GetAllAsync();
        Crews = new ObservableCollection<CrewDto>(crews);
    }

    [RelayCommand]
    private async Task CreateCrew()
    {
        if (string.IsNullOrWhiteSpace(NewCrewName)) return;

        var request = new CreateCrewRequest
        {
            Name = NewCrewName,
            Resources = []
        };

        await _crewService.CreateAsync(request);
        NewCrewName = string.Empty;
        StatusMessage = "Crew created";
        await LoadCrews();
    }

    [RelayCommand]
    private async Task CloneCrew(CrewDto? crew)
    {
        if (crew is null) return;
        await _crewService.CloneAsync(crew.Id, $"{crew.Name} (Copy)");
        StatusMessage = "Crew cloned";
        await LoadCrews();
    }

    [RelayCommand]
    private async Task DeleteCrew(CrewDto? crew)
    {
        if (crew is null) return;
        await _crewService.DeleteAsync(crew.Id);
        StatusMessage = "Crew deleted";
        await LoadCrews();
    }
}
