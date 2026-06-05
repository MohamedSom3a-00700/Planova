using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class ActivityBankBrowserViewModel : ObservableObject
{
    private readonly IActivityBankService _bankService;
    private readonly IWbsGenerationService _wbsGenerationService;

    public ActivityBankBrowserViewModel(IActivityBankService bankService, IWbsGenerationService wbsGenerationService)
    {
        _bankService = bankService;
        _wbsGenerationService = wbsGenerationService;
    }

    [ObservableProperty]
    private ObservableCollection<string> _categories = [];

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<ActivityBankDto> _entries = [];

    [ObservableProperty]
    private ActivityBankDto? _selectedEntry;

    [ObservableProperty]
    private ActivityBankDto? _previewEntry;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ActivityPreviewItem> _generatedPreviews = [];

    [ObservableProperty]
    private bool _hasGeneratedPreviews;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task BrowseCategoryAsync(CancellationToken ct)
    {
        var entries = await _bankService.BrowseAsync(SelectedCategory, SearchText, ct);
        Entries = new ObservableCollection<ActivityBankDto>(entries);
    }

    [RelayCommand]
    private async Task PreviewEntryAsync(CancellationToken ct)
    {
        if (SelectedEntry is null) return;
        PreviewEntry = await _bankService.GetByIdAsync(SelectedEntry.Id, ct);
    }

    [RelayCommand]
    private async Task ApplyToWbsAsync(CancellationToken ct)
    {
        if (SelectedEntry is null) return;

        var selectedWbsIds = new List<Guid>();

        StatusMessage = "Applied bank entry to WBS items successfully.";
    }

    [RelayCommand]
    private async Task SaveAsCustomEntryAsync(CancellationToken ct)
    {
        if (SelectedEntry is null) return;

        StatusMessage = "Custom bank entry saved successfully.";
    }

    [RelayCommand]
    private async Task ImportFromExcelAsync(CancellationToken ct)
    {
        StatusMessage = "Import from Excel — coming in Phase 6.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportToExcelAsync(CancellationToken ct)
    {
        if (SelectedEntry is null) { StatusMessage = "Select an entry first."; return; }
        StatusMessage = "Export to Excel — coming in Phase 6.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AiGenerateBankEntryAsync(CancellationToken ct)
    {
        StatusMessage = "AI generation — coming in Phase 6.";
        await Task.CompletedTask;
    }
}
