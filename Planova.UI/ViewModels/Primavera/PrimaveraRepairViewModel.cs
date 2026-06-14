using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraRepairViewModel : ObservableObject
{
    private readonly IPrimaveraRepairService _repairService;

    public PrimaveraRepairViewModel(IPrimaveraRepairService repairService)
    {
        _repairService = repairService;
    }

    public ObservableCollection<PrimaveraRepairActionDto> SuggestedFixes { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadSuggestionsAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading suggestions...";
        SuggestedFixes.Clear();

        try
        {
            var fixes = await _repairService.GetSuggestedFixesAsync(1);
            foreach (var fix in fixes)
                SuggestedFixes.Add(fix);

            StatusMessage = fixes.Count == 0
                ? "No suggested fixes available."
                : $"{fixes.Count} fix(es) available.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyAllFixesAsync()
    {
        IsLoading = true;
        try
        {
            var success = await _repairService.ApplyAllFixesAsync(1);
            StatusMessage = success ? "All fixes applied." : "Some fixes could not be applied.";
            SuggestedFixes.Clear();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
