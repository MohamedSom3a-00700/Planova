using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class WbsGenerationWizardViewModel : ObservableObject
{
    private readonly IWbsGenerationService _wbsGenerationService;

    public WbsGenerationWizardViewModel(IWbsGenerationService wbsGenerationService)
    {
        _wbsGenerationService = wbsGenerationService;
    }

    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private string _generationMode = "Simple";

    [ObservableProperty]
    private bool _isSimpleGeneration = true;

    [ObservableProperty]
    private bool _isBankGeneration;

    [ObservableProperty]
    private bool _isBOQGeneration;

    partial void OnIsSimpleGenerationChanged(bool value)
    {
        if (value) GenerationMode = "Simple";
    }

    partial void OnIsBankGenerationChanged(bool value)
    {
        if (value) GenerationMode = "Bank";
    }

    partial void OnIsBOQGenerationChanged(bool value)
    {
        if (value) GenerationMode = "BOQ";
    }

    [ObservableProperty]
    private List<Guid> _selectedWbsItems = [];

    [ObservableProperty]
    private Guid? _selectedBankId;

    [ObservableProperty]
    private WbsGenerationPreviewDto? _preview;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private int _progressPercentage;

    [ObservableProperty]
    private string _progressMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _availableBoqItems = [];

    [ObservableProperty]
    private string? _selectedBoqItem;

    [ObservableProperty]
    private List<string> _selectedBoqCodes = [];

    [RelayCommand]
    private async Task LoadBoqItemsAsync(CancellationToken ct)
    {
        ProgressMessage = "BOQ load — coming in Phase 6.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NextStepAsync(CancellationToken ct)
    {
        if (CurrentStep == 1 && GenerationMode == "Simple")
        {
            Preview = await _wbsGenerationService.PreviewSimpleGenerationAsync(SelectedWbsItems, ct);
        }
        else if (CurrentStep == 1 && GenerationMode == "Bank" && SelectedBankId.HasValue)
        {
            Preview = await _wbsGenerationService.PreviewBankGenerationAsync(SelectedWbsItems, SelectedBankId.Value, ct);
        }

        CurrentStep++;
    }

    [RelayCommand]
    private async Task CommitGenerationAsync(CancellationToken ct)
    {
        IsGenerating = true;
        try
        {
            var request = new WbsGenerationRequest
            {
                ProjectId = 1,
                Mode = GenerationMode,
                WbsItemIds = SelectedWbsItems,
                BankId = SelectedBankId
            };
            await _wbsGenerationService.CommitGenerationAsync(request, ct);
            ProgressMessage = "Generation complete!";
            ProgressPercentage = 100;
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
