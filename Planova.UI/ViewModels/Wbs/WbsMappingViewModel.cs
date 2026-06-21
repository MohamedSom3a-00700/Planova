using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsMappingViewModel : ObservableObject
{
    private readonly IBoqService _boqService;
    private readonly IWbsMappingService _mappingService;
    private readonly IBoqImportService _importService;
    private readonly IBoqSession _session;

    public WbsMappingViewModel(
        IBoqService boqService,
        IWbsMappingService mappingService,
        IBoqImportService importService,
        IBoqSession session)
    {
        _boqService = boqService;
        _mappingService = mappingService;
        _importService = importService;
        _session = session;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _currentStep = 1;

    public bool IsStep1Visible => CurrentStep == 1;
    public bool IsStep2Visible => CurrentStep == 2;
    public bool IsStep3Visible => CurrentStep == 3;
    public bool IsStep4Visible => CurrentStep == 4;
    public bool CanGoBack => CurrentStep > 1;
    public bool CanGoNext => CurrentStep switch
    {
        1 => SelectedBoq is not null,
        2 => SelectedStrategy is not null,
        3 => PreviewNodes.Count > 0,
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

    public List<MappingStrategy> Strategies { get; } = new()
    {
        new("By Section", "Maps BOQ sections to WBS root nodes"),
        new("By CSI", "Groups BOQ items by CSI code into WBS hierarchy"),
        new("By Cost Code", "Groups BOQ items by cost code into WBS hierarchy"),
        new("By Trade", "Groups BOQ items by trade code into WBS hierarchy"),
        new("By Discipline", "Groups BOQ items by discipline into WBS hierarchy"),
    };

    [ObservableProperty]
    private MappingStrategy? _selectedStrategy;

    [ObservableProperty]
    private string _wbsName = string.Empty;

    [ObservableProperty]
    private WbsMappingPreview? _mappingPreview;

    public ObservableCollection<PreviewNodeItem> PreviewNodes { get; } = new();

    public List<string> StepTitles { get; } = new()
    {
        "Select BOQ", "Choose Strategy", "Preview", "Commit"
    };

    partial void OnSelectedBoqChanged(BoqSummaryDto? value)
    {
        WbsName = value?.Name is not null ? $"WBS - {value.Name}" : string.Empty;
        OnPropertyChanged(nameof(CanGoNext));
    }

    partial void OnSelectedStrategyChanged(MappingStrategy? value)
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

    private static WbsMappingMethod ToMappingMethod(string name) => name switch
    {
        "By Section" => WbsMappingMethod.BySection,
        "By CSI" => WbsMappingMethod.ByCsi,
        "By Cost Code" => WbsMappingMethod.ByCostCode,
        "By Trade" => WbsMappingMethod.ByTrade,
        "By Discipline" => WbsMappingMethod.ByDiscipline,
        _ => WbsMappingMethod.BySection
    };

    [RelayCommand]
    private async Task GeneratePreviewAsync(CancellationToken ct)
    {
        if (SelectedBoq is null || SelectedStrategy is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var method = ToMappingMethod(SelectedStrategy.Name);
            MappingPreview = await _mappingService.PreviewMappingAsync(SelectedBoq.Id, method, ct);

            PreviewNodes.Clear();
            FlattenNodes(MappingPreview.Nodes, 0);

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

    private void FlattenNodes(IReadOnlyList<WbsMappingNode> nodes, int level)
    {
        foreach (var node in nodes)
        {
            PreviewNodes.Add(new PreviewNodeItem(node.Code, node.Name, level, node.Weight));
            if (node.Children.Count > 0)
                FlattenNodes(node.Children, level + 1);
        }
    }

    [RelayCommand]
    private async Task CommitMappingAsync(CancellationToken ct)
    {
        if (SelectedBoq is null || SelectedStrategy is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var method = ToMappingMethod(SelectedStrategy.Name);
            var name = string.IsNullOrWhiteSpace(WbsName) ? $"WBS from {SelectedStrategy.Name}" : WbsName;
            await _mappingService.CreateWbsFromMappingAsync(SelectedBoq.Id, method, name, 0, ct);

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
        SelectedStrategy = null;
        WbsName = string.Empty;
        MappingPreview = null;
        PreviewNodes.Clear();
        ErrorMessage = string.Empty;
        HasError = false;
    }
}

public sealed record MappingStrategy(string Name, string Description);

public sealed partial class PreviewNodeItem : ObservableObject
{
    public PreviewNodeItem(string code, string name, int level, decimal? weight)
    {
        Code = code;
        Name = name;
        Level = level;
        Weight = weight;
    }

    public string Code { get; }
    public string Name { get; }
    public int Level { get; }
    public decimal? Weight { get; }
}
