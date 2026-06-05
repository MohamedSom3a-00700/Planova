using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceAssignmentViewModel : ObservableObject
{
    private readonly IResourceAssignmentService _assignmentService;

    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private ObservableCollection<ResourceAssignmentDto> _assignments = new();

    [ObservableProperty]
    private decimal _totalCost;

    public ResourceAssignmentViewModel(IResourceAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [RelayCommand]
    private async Task LoadAssignments()
    {
        if (ActivityId == Guid.Empty) return;

        var items = await _assignmentService.GetByActivityAsync(ActivityId);
        Assignments = new ObservableCollection<ResourceAssignmentDto>(items);
        TotalCost = await _assignmentService.GetActivityTotalCostAsync(ActivityId);
    }

    [RelayCommand]
    private async Task AddAssignment()
    {
        if (ActivityId == Guid.Empty) return;

        var request = new CreateAssignmentRequest
        {
            ActivityId = ActivityId,
            ResourceId = Guid.Empty,
            Quantity = 1,
            Rate = 0,
            Currency = "USD",
            UnitOfMeasure = "hr"
        };

        await _assignmentService.CreateAsync(request);
        await LoadAssignments();
    }

    [RelayCommand]
    private async Task RemoveAssignment(ResourceAssignmentDto? assignment)
    {
        if (assignment is null) return;
        await _assignmentService.DeleteAsync(assignment.Id);
        await LoadAssignments();
    }

    [RelayCommand]
    private async Task ApplyCrew()
    {
        await Task.CompletedTask;
    }
}
