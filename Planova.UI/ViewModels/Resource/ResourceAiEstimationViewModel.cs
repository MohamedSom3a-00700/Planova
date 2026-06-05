using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceAiEstimationViewModel : ObservableObject
{
    private readonly IResourceAiEstimationService _aiService;

    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private bool _isEstimating;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private ObservableCollection<AiSuggestionDto> _suggestions = new();

    [ObservableProperty]
    private string? _errorMessage;

    public ResourceAiEstimationViewModel(IResourceAiEstimationService aiService)
    {
        _aiService = aiService;
    }

    [RelayCommand]
    private async Task EstimateResources()
    {
        if (ActivityId == Guid.Empty) return;

        IsAvailable = await _aiService.IsAvailableAsync();
        if (!IsAvailable)
        {
            ErrorMessage = "AI estimation is not available. Configure a Semantic Kernel provider in settings.";
            return;
        }

        IsEstimating = true;
        ErrorMessage = null;
        try
        {
            var results = await _aiService.EstimateResourcesAsync(ActivityId);
            Suggestions = new ObservableCollection<AiSuggestionDto>(results);
            if (results.Count == 0)
                ErrorMessage = "No suggestions returned. Try with a more detailed activity description.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Estimation failed: {ex.Message}";
        }
        finally
        {
            IsEstimating = false;
        }
    }

    [RelayCommand]
    private async Task AcceptAll()
    {
        if (Suggestions.Count == 0) return;
        var accepted = Suggestions.Select(s => new AcceptedSuggestionDto
        {
            ResourceCode = s.ResourceCode,
            Quantity = s.SuggestedQuantity
        }).ToList();
        await _aiService.AcceptSuggestionsAsync(ActivityId, accepted);
        Suggestions.Clear();
    }

    [RelayCommand]
    private async Task RejectAll()
    {
        Suggestions.Clear();
        ErrorMessage = null;
    }
}
