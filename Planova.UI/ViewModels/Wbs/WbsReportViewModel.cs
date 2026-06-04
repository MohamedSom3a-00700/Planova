using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;
using Microsoft.Win32;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsReportViewModel : ObservableObject
{
    private readonly IWbsReportService _reportService;

    public WbsReportViewModel(IWbsReportService reportService)
    {
        _reportService = reportService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private Guid _selectedWbsId;

    [ObservableProperty]
    private int _selectedReportTypeIndex;

    [ObservableProperty]
    private WbsSummaryReport? _summaryReport;

    [ObservableProperty]
    private WbsDictionaryReport? _dictionaryReport;

    public bool IsSummarySelected => SelectedReportTypeIndex == 0;
    public bool IsDictionarySelected => SelectedReportTypeIndex == 1;

    public ObservableCollection<ReportSection> SummarySections { get; } = new();
    public ObservableCollection<DictionaryEntry> DictionaryEntries { get; } = new();

    partial void OnSelectedReportTypeIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsSummarySelected));
        OnPropertyChanged(nameof(IsDictionarySelected));
    }

    [RelayCommand]
    private async Task LoadSummaryAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        IsLoading = true;
        HasError = false;

        try
        {
            SummaryReport = await _reportService.GenerateSummaryAsync(SelectedWbsId, ct);
            SummarySections.Clear();
            if (SummaryReport is not null)
            {
                foreach (var section in SummaryReport.Sections)
                    SummarySections.Add(section);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load summary: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadDictionaryAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        IsLoading = true;
        HasError = false;

        try
        {
            DictionaryReport = await _reportService.GenerateDictionaryAsync(SelectedWbsId, ct);
            DictionaryEntries.Clear();
            if (DictionaryReport is not null)
            {
                foreach (var entry in DictionaryReport.Entries)
                    DictionaryEntries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dictionary: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files|*.csv|All Files|*.*",
            Title = "Export WBS Report",
            FileName = IsSummarySelected ? "WBS_Summary.csv" : "WBS_Dictionary.csv"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var reportType = IsSummarySelected ? ReportType.Summary : ReportType.Dictionary;
            var data = await _reportService.ExportToExcelAsync(SelectedWbsId, reportType, ct);
            await File.WriteAllBytesAsync(dialog.FileName, data, ct);
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

    [RelayCommand]
    private async Task ExportPdfAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        var dialog = new SaveFileDialog
        {
            Filter = "PDF Files|*.pdf|All Files|*.*",
            Title = "Export WBS Report",
            FileName = IsSummarySelected ? "WBS_Summary.pdf" : "WBS_Dictionary.pdf"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var reportType = IsSummarySelected ? ReportType.Summary : ReportType.Dictionary;
            var data = await _reportService.ExportToPdfAsync(SelectedWbsId, reportType, ct);
            await File.WriteAllBytesAsync(dialog.FileName, data, ct);
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
