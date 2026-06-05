using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class RelationshipEditorViewModel : ObservableObject
{
    private readonly IActivityRelationshipService _relationshipService;

    public RelationshipEditorViewModel(IActivityRelationshipService relationshipService)
    {
        _relationshipService = relationshipService;
    }

    [ObservableProperty]
    private ObservableCollection<ActivityRelationshipDto> _relationships = [];

    [ObservableProperty]
    private ActivityRelationshipDto? _selectedRelationship;

    [ObservableProperty]
    private List<ActivityDto> _availableActivities = [];

    [ObservableProperty]
    private Guid? _selectedPredecessorId;

    [ObservableProperty]
    private Guid? _selectedSuccessorId;

    [ObservableProperty]
    private string _selectedType = "FS";

    [ObservableProperty]
    private int _lagDays;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task LoadFromExcelAsync(CancellationToken ct)
    {
        StatusMessage = "Load from Excel — coming in Phase 6.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AiGenerateRelationshipsAsync(CancellationToken ct)
    {
        StatusMessage = "AI generation — coming in Phase 6.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CreateRelationshipAsync(CancellationToken ct)
    {
        if (SelectedPredecessorId is null || SelectedSuccessorId is null) return;

        var request = new CreateRelationshipRequest
        {
            ProjectId = 1,
            PredecessorId = SelectedPredecessorId.Value,
            SuccessorId = SelectedSuccessorId.Value,
            Type = SelectedType,
            LagDays = LagDays
        };

        var result = await _relationshipService.ValidateNewRelationshipAsync(request.PredecessorId, request.SuccessorId, ct);
        if (result.HasCycle)
        {
            ErrorMessage = result.Message;
            return;
        }

        var dto = await _relationshipService.CreateAsync(request, ct);
        Relationships.Add(dto);
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task DeleteRelationshipAsync(CancellationToken ct)
    {
        if (SelectedRelationship is null) return;
        await _relationshipService.DeleteAsync(SelectedRelationship.Id, ct);
        Relationships.Remove(SelectedRelationship);
    }

    [RelayCommand]
    private async Task ValidateRelationshipAsync(CancellationToken ct)
    {
        if (SelectedPredecessorId is null || SelectedSuccessorId is null) return;

        var result = await _relationshipService.ValidateNewRelationshipAsync(
            SelectedPredecessorId.Value, SelectedSuccessorId.Value, ct);
        ErrorMessage = result.HasCycle ? result.Message : null;
    }
}
