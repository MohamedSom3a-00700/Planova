using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceAssignmentViewModel : ObservableObject
{
    private readonly IResourceAssignmentService _assignmentService;
    private readonly IResourceService _resourceService;
    private readonly ICrewService _crewService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private ObservableCollection<ResourceAssignmentDto> _assignments = new();

    [ObservableProperty]
    private ObservableCollection<ResourceDto> _availableResources = new();

    [ObservableProperty]
    private ResourceDto? _selectedNewResource;

    [ObservableProperty]
    private decimal _newQuantity = 1;

    [ObservableProperty]
    private decimal _newRate;

    [ObservableProperty]
    private ObservableCollection<CrewDto> _availableCrews = new();

    [ObservableProperty]
    private CrewDto? _selectedCrew;

    [ObservableProperty]
    private decimal _totalCost;

    public ResourceAssignmentViewModel(
        IResourceAssignmentService assignmentService,
        IResourceService resourceService,
        ICrewService crewService,
        ICurrentProjectService currentProjectService)
    {
        _assignmentService = assignmentService;
        _resourceService = resourceService;
        _crewService = crewService;
        _currentProjectService = currentProjectService;
        _currentProjectService.CurrentProjectChanged += OnProjectChanged;
        _ = LoadDataAsync();
    }

    private async void OnProjectChanged(object? sender, ProjectContext? project)
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId.HasValue)
        {
            AvailableResources = new ObservableCollection<ResourceDto>(
                await _resourceService.GetByProjectAsync(projectId.Value));
            AvailableCrews = new ObservableCollection<CrewDto>(
                await _crewService.GetAllAsync(projectId));
        }

        if (ActivityId != Guid.Empty)
            await LoadAssignmentsAsync();
    }

    [RelayCommand]
    private async Task LoadAssignmentsAsync()
    {
        if (ActivityId == Guid.Empty) return;

        var items = await _assignmentService.GetByActivityAsync(ActivityId);
        Assignments = new ObservableCollection<ResourceAssignmentDto>(items);
        TotalCost = await _assignmentService.GetActivityTotalCostAsync(ActivityId);
    }

    [RelayCommand]
    private async Task AddAssignment()
    {
        var projectId = _currentProjectService.CurrentProject?.Id ?? 0;
        if (projectId <= 0) return;

        if (ActivityId == Guid.Empty)
            ActivityId = Guid.NewGuid();

        var resourceId = SelectedNewResource?.Id ?? Guid.Empty;
        if (resourceId == Guid.Empty) return;

        var request = new CreateAssignmentRequest
        {
            ProjectId = projectId,
            ActivityId = ActivityId,
            ResourceId = resourceId,
            Quantity = NewQuantity,
            Rate = NewRate,
            Currency = "USD",
            UnitOfMeasure = "hr"
        };

        await _assignmentService.CreateAsync(request);
        NewQuantity = 1;
        NewRate = 0;
        await LoadAssignmentsAsync();
    }

    [RelayCommand]
    private async Task RemoveAssignment(ResourceAssignmentDto? assignment)
    {
        if (assignment is null) return;
        await _assignmentService.DeleteAsync(assignment.Id);
        await LoadAssignmentsAsync();
    }

    [RelayCommand]
    private async Task ApplyCrew()
    {
        if (SelectedCrew is null) return;
        if (ActivityId == Guid.Empty) return;

        await _crewService.ApplyToActivitiesAsync(SelectedCrew.Id, [ActivityId]);
        await LoadAssignmentsAsync();
    }
}
