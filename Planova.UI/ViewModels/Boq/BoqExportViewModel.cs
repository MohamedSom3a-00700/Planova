using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;
using AppServices = Planova.Boq.Application.Services;

namespace Planova.UI.ViewModels.Boq;

public sealed partial class BoqExportViewModel : ObservableObject
{
    private readonly IBoqExportService _exportService;
    private readonly IBoqService _boqService;
    private readonly IBoqSession _session;

    public BoqExportViewModel(
        IBoqExportService exportService,
        IBoqService boqService,
        IBoqSession session)
    {
        _exportService = exportService;
        _boqService = boqService;
        _session = session;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _selectedExportTypeIndex;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    public ObservableCollection<BoqSummaryDto> Boqs { get; } = new();

    [ObservableProperty]
    private BoqSummaryDto? _selectedBoq;

    public bool IsExcelSelected => SelectedExportTypeIndex == 0;
    public bool IsCsvSelected => SelectedExportTypeIndex == 1;
    public bool IsPdfSelected => SelectedExportTypeIndex == 2;
    public bool IsTenderBoqSelected => SelectedExportTypeIndex == 3;
    public bool IsClientBoqSelected => SelectedExportTypeIndex == 4;

    partial void OnSelectedExportTypeIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsExcelSelected));
        OnPropertyChanged(nameof(IsCsvSelected));
        OnPropertyChanged(nameof(IsPdfSelected));
        OnPropertyChanged(nameof(IsTenderBoqSelected));
        OnPropertyChanged(nameof(IsClientBoqSelected));
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
            Boqs.Clear();
            foreach (var boq in boqs)
                Boqs.Add(boq);
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
    private async Task ExportAsync(CancellationToken ct)
    {
        if (SelectedBoq is null)
        {
            ErrorMessage = "Please select a BOQ to export.";
            HasError = true;
            return;
        }

        IsLoading = true;
        HasError = false;

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = SelectedExportTypeIndex switch
                {
                    0 => "Excel Files|*.xlsx",
                    1 => "CSV Files|*.csv",
                    2 => "PDF Files|*.pdf",
                    3 or 4 => "Excel Files|*.xlsx",
                    _ => "All Files|*.*"
                },
                FileName = $"{SelectedBoq.Name}.xlsx"
            };

            if (dialog.ShowDialog() != true) return;

            var options = new ExportOptions(true, true, true, null, dialog.FileName);

            ExportResult result = SelectedExportTypeIndex switch
            {
                0 => await _exportService.ExportToExcelAsync(SelectedBoq.Id, options, ct),
                1 => await _exportService.ExportToCsvAsync(SelectedBoq.Id, options, ct),
                2 => await _exportService.ExportToPdfAsync(SelectedBoq.Id, options, ct),
                3 => await _exportService.ExportTenderBoqAsync(SelectedBoq.Id, options, ct),
                4 => await _exportService.ExportClientBoqAsync(SelectedBoq.Id, options, ct),
                _ => await _exportService.ExportToExcelAsync(SelectedBoq.Id, options, ct)
            };

            OutputPath = result.OutputPath;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
