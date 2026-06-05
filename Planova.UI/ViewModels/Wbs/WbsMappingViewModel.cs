using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsMappingViewModel : ObservableObject
{
    private readonly IBoqService _boqService;
    private readonly IWbsBoqMappingService _mappingService;
    private readonly IBoqImportService _importService;
    private readonly IBoqSession _session;

    public WbsMappingViewModel(
        IBoqService boqService,
        IWbsBoqMappingService mappingService,
        IBoqImportService importService,
        IBoqSession session)
    {
        _boqService = boqService;
        _mappingService = mappingService;
        _importService = importService;
        _session = session;
        WbsProjectId = session.CurrentProjectId?.GetHashCode() ?? 0;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private int _wbsProjectId;

    public bool IsStep1Visible => CurrentStep == 1;
    public bool IsStep2Visible => CurrentStep == 2;
    public bool IsStep3Visible => CurrentStep == 3;
    public bool IsStep4Visible => CurrentStep == 4;
    public bool CanGoBack => CurrentStep > 1;
    public bool CanGoNext => CurrentStep switch
    {
        1 => SelectedBoq is not null,
        2 => !string.IsNullOrWhiteSpace(SelectedStrategy),
        3 => MappedItems.Count > 0,
        _ => false
    };

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsStep1Visible));
        OnPropertyChanged(nameof(IsStep2Visible));
        OnPropertyChanged(nameof(IsStep3Visible));
        OnPropertyChanged(nameof(IsStep4Visible));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
    }

    public ObservableCollection<BoqSummaryDto> AvailableBoqs { get; } = new();

    [ObservableProperty]
    private BoqSummaryDto? _selectedBoq;

    public List<string> Strategies { get; } = new()
    {
        "OneToOne", "Grouped"
    };

    [ObservableProperty]
    private string _selectedStrategy = string.Empty;

    [ObservableProperty]
    private string _wbsName = string.Empty;

    [ObservableProperty]
    private WbsMappingResult? _mappingResult;

    public ObservableCollection<MappedItemViewModel> MappedItems { get; } = new();

    public List<string> StepTitles { get; } = new()
    {
        "Select BOQ", "Choose Strategy", "Preview", "Commit"
    };

    partial void OnSelectedBoqChanged(BoqSummaryDto? value)
    {
        WbsName = value?.Name is not null ? $"WBS - {value.Name}" : string.Empty;
        OnPropertyChanged(nameof(CanGoNext));
    }

    partial void OnSelectedStrategyChanged(string value)
    {
        OnPropertyChanged(nameof(CanGoNext));
    }

    [RelayCommand]
    private async Task ImportBoqFromExcelAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx;*.xls;*.xlsm|All Files|*.*",
            Title = "Import BOQ from Excel"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            await _importService.ImportFromExcelAsync(projectId.Value, dialog.FileName, null, new Progress<int>(), ct);
            await LoadBoqsAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import BOQ from Excel: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadBoqsAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var boqs = await _boqService.GetByProjectIdAsync(projectId.Value, ct);
            AvailableBoqs.Clear();
            foreach (var boq in boqs)
                AvailableBoqs.Add(boq);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load BOQs: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GeneratePreviewAsync(CancellationToken ct)
    {
        if (SelectedBoq is null || string.IsNullOrWhiteSpace(SelectedStrategy)) return;

        IsLoading = true;
        HasError = false;

        try
        {
            WbsMappingResult result = SelectedStrategy switch
            {
                "OneToOne" => await _mappingService.MapOneToOneAsync(SelectedBoq.Id, ct),
                "Grouped" => await _mappingService.MapGroupedAsync(SelectedBoq.Id, ct),
                _ => throw new InvalidOperationException($"Unknown strategy: {SelectedStrategy}")
            };

            MappingResult = result;
            MappedItems.Clear();
            foreach (var item in result.Items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder))
                MappedItems.Add(new MappedItemViewModel(item));

            CurrentStep = 3;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Mapping failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CommitMappingAsync(CancellationToken ct)
    {
        if (MappingResult is null || MappedItems.Count == 0) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var finalResult = new WbsMappingResult(
                MappedItems.Select(m => m.ToMappedItem()).ToList(),
                MappingResult.Strategy);

            var name = string.IsNullOrWhiteSpace(WbsName) ? $"WBS from {SelectedStrategy}" : WbsName;
            await _mappingService.CommitMappingAsync(finalResult, name, WbsProjectId, ct);

            CurrentStep = 4;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Commit failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NextStep()
    {
        if (!CanGoNext) return;
        if (CurrentStep < 4) CurrentStep++;
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1) CurrentStep--;
    }

    [RelayCommand]
    private void Reset()
    {
        CurrentStep = 1;
        SelectedBoq = null;
        SelectedStrategy = string.Empty;
        WbsName = string.Empty;
        MappingResult = null;
        MappedItems.Clear();
        ErrorMessage = string.Empty;
        HasError = false;
    }
}

public sealed partial class MappedItemViewModel : ObservableObject
{
    public MappedItemViewModel(MappedItem item)
    {
        TargetId = item.TargetId;
        SourceBoqItemId = item.SourceBoqItemId;
        ParentTargetId = item.ParentTargetId;
        Name = item.Name;
        Level = item.Level;
        SortOrder = item.SortOrder;
        WbsLevel = item.WbsLevel;
    }

    public Guid? TargetId { get; }
    public Guid SourceBoqItemId { get; }
    public Guid? ParentTargetId { get; }
    public int Level { get; }
    public int SortOrder { get; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _wbsLevel = string.Empty;

    public MappedItem ToMappedItem() => new(
        TargetId, SourceBoqItemId, ParentTargetId, Name, Level, SortOrder, WbsLevel);
}
