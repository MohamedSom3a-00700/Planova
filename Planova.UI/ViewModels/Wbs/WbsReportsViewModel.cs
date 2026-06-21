using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;
using Microsoft.Win32;
using WbsItem = Planova.Wbs.Domain.Entities.WbsItem;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsReportsViewModel : ObservableObject
{
    private readonly IWbsReportService _reportService;
    private readonly IWbsService _wbsService;

    public WbsReportsViewModel(IWbsReportService reportService, IWbsService wbsService)
    {
        _reportService = reportService;
        _wbsService = wbsService;
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
    private int _selectedReportTab;

    public bool IsSummaryTab => SelectedReportTab == 0;
    public bool IsDictionaryTab => SelectedReportTab == 1;
    public bool IsWeightTab => SelectedReportTab == 2;
    public bool IsResponsibilityTab => SelectedReportTab == 3;
    public bool IsHierarchyTab => SelectedReportTab == 4;

    partial void OnSelectedReportTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsSummaryTab));
        OnPropertyChanged(nameof(IsDictionaryTab));
        OnPropertyChanged(nameof(IsWeightTab));
        OnPropertyChanged(nameof(IsResponsibilityTab));
        OnPropertyChanged(nameof(IsHierarchyTab));
    }

    [ObservableProperty]
    private WbsSummaryReport? _summaryReport;

    [ObservableProperty]
    private WbsDictionaryReport? _dictionaryReport;

    public ObservableCollection<ReportSection> SummarySections { get; } = new();
    public ObservableCollection<DictionaryEntry> DictionaryEntries { get; } = new();

    // Weight Report
    public ObservableCollection<WeightReportItem> WeightReportItems { get; } = new();

    // Responsibility Matrix
    public ObservableCollection<ResponsibilityMatrixItem> ResponsibilityMatrixItems { get; } = new();

    // Hierarchy Report
    public ObservableCollection<HierarchyReportItem> HierarchyReportItems { get; } = new();

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
    private async Task LoadWeightReportAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;
        IsLoading = true;
        HasError = false;

        try
        {
            var items = await _wbsService.GetTreeAsync(SelectedWbsId, ct);
            var lookup = items.ToDictionary(i => i.Id);
            var sorted = items.OrderByDescending(i => i.Weight ?? 0).ToList();

            WeightReportItems.Clear();
            foreach (var item in sorted)
            {
                var parentName = item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent)
                    ? parent.Name
                    : null;

                WeightReportItems.Add(new WeightReportItem(
                    item.Code, item.Name, item.Weight, parentName));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load weight report: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadResponsibilityMatrixAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;
        IsLoading = true;
        HasError = false;

        try
        {
            var items = await _wbsService.GetTreeAsync(SelectedWbsId, ct);
            var sorted = items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder).ToList();

            ResponsibilityMatrixItems.Clear();
            foreach (var item in sorted)
            {
                ResponsibilityMatrixItems.Add(new ResponsibilityMatrixItem(
                    item.Code, item.Name, item.AssignedTo, item.Discipline));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load responsibility matrix: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadHierarchyReportAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;
        IsLoading = true;
        HasError = false;

        try
        {
            var items = await _wbsService.GetTreeAsync(SelectedWbsId, ct);
            var roots = items.Where(i => i.ParentId is null).OrderBy(i => i.SortOrder).ToList();

            HierarchyReportItems.Clear();
            foreach (var root in roots)
                AddHierarchyItem(items, root, 0);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load hierarchy report: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddHierarchyItem(IReadOnlyList<WbsItem> allItems, WbsItem item, int depth)
    {
        HierarchyReportItems.Add(new HierarchyReportItem(
            item.Code, item.Name, item.Level, depth, item.Weight));

        var children = allItems.Where(i => i.ParentId == item.Id).OrderBy(i => i.SortOrder).ToList();
        foreach (var child in children)
            AddHierarchyItem(allItems, child, depth + 1);
    }

    private string GetExportFileName(string extension) => SelectedReportTab switch
    {
        0 => $"WBS_Summary.{extension}",
        1 => $"WBS_Dictionary.{extension}",
        2 => $"WBS_WeightReport.{extension}",
        3 => $"WBS_ResponsibilityMatrix.{extension}",
        4 => $"WBS_HierarchyReport.{extension}",
        _ => $"WBS_Report.{extension}"
    };

    [RelayCommand]
    private async Task ExportExcelAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        var dialog = new SaveFileDialog
        {
            Filter = "Excel Files|*.xlsx|All Files|*.*",
            FileName = GetExportFileName("xlsx")
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            if (SelectedReportTab <= 1)
            {
                var reportType = SelectedReportTab == 0 ? ReportType.Summary : ReportType.Dictionary;
                var data = await _reportService.ExportToExcelAsync(SelectedWbsId, reportType, ct);
                await File.WriteAllBytesAsync(dialog.FileName, data, ct);
            }
            else
            {
                var csv = GenerateCsv();
                await File.WriteAllTextAsync(dialog.FileName, csv, ct);
            }
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
            FileName = GetExportFileName("pdf")
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            if (SelectedReportTab <= 1)
            {
                var reportType = SelectedReportTab == 0 ? ReportType.Summary : ReportType.Dictionary;
                var data = await _reportService.ExportToPdfAsync(SelectedWbsId, reportType, ct);
                await File.WriteAllBytesAsync(dialog.FileName, data, ct);
            }
            else
            {
                var csv = GenerateCsv();
                await File.WriteAllTextAsync(dialog.FileName, csv, ct);
            }
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
    private async Task ExportCsvAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files|*.csv|All Files|*.*",
            FileName = GetExportFileName("csv")
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var csv = GenerateCsv();
            await File.WriteAllTextAsync(dialog.FileName, csv, ct);
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
    private async Task ExportJsonAsync(CancellationToken ct)
    {
        if (SelectedWbsId == Guid.Empty) return;

        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            FileName = GetExportFileName("json")
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var json = GenerateJson();
            await File.WriteAllTextAsync(dialog.FileName, json, ct);
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

    private string GenerateCsv()
    {
        using var writer = new StringWriter();
        switch (SelectedReportTab)
        {
            case 2:
                writer.WriteLine("Code,Name,Weight%,Parent");
                foreach (var item in WeightReportItems)
                    writer.WriteLine($"{item.Code},{item.Name},{item.WeightPercentage},{item.Parent}");
                break;
            case 3:
                writer.WriteLine("Code,Name,AssignedTo,Discipline");
                foreach (var item in ResponsibilityMatrixItems)
                    writer.WriteLine($"{item.Code},{item.Name},{item.AssignedTo},{item.Discipline}");
                break;
            case 4:
                writer.WriteLine("Code,Name,Level,Indent,Weight%");
                foreach (var item in HierarchyReportItems)
                    writer.WriteLine($"{item.Code},{item.Name},{item.Level},{item.Indent},{item.WeightPercentage}");
                break;
            default:
                if (SelectedReportTab == 0 && SummaryReport is not null)
                {
                    writer.WriteLine($"WBS Summary: {SummaryReport.WbsName}");
                    writer.WriteLine("Level,Items,Weight%");
                    foreach (var s in SummarySections)
                        writer.WriteLine($"{s.Name},{s.ItemCount},{s.WeightPercentage}");
                }
                else if (SelectedReportTab == 1 && DictionaryReport is not null)
                {
                    writer.WriteLine($"WBS Dictionary: {DictionaryReport.WbsName}");
                    writer.WriteLine("Code,Name,Level,AssignedTo,Deliverable");
                    foreach (var e in DictionaryEntries)
                        writer.WriteLine($"{e.Code},{e.Name},{e.WbsLevel},{e.AssignedTo},{e.Deliverable}");
                }
                break;
        }
        return writer.ToString();
    }

    private string GenerateJson()
    {
        return SelectedReportTab switch
        {
            2 => JsonSerializer.Serialize(WeightReportItems.ToList(), new JsonSerializerOptions { WriteIndented = true }),
            3 => JsonSerializer.Serialize(ResponsibilityMatrixItems.ToList(), new JsonSerializerOptions { WriteIndented = true }),
            4 => JsonSerializer.Serialize(HierarchyReportItems.ToList(), new JsonSerializerOptions { WriteIndented = true }),
            _ => JsonSerializer.Serialize(new { Summary = SummaryReport, Dictionary = DictionaryReport }, new JsonSerializerOptions { WriteIndented = true })
        };
    }
}

public sealed record WeightReportItem(
    string Code,
    string Name,
    decimal? WeightPercentage,
    string? Parent
);

public sealed record ResponsibilityMatrixItem(
    string Code,
    string Name,
    string? AssignedTo,
    string? Discipline
);

public sealed record HierarchyReportItem(
    string Code,
    string Name,
    int Level,
    int Indent,
    decimal? WeightPercentage
)
{
    public string IndentedName => new string(' ', Indent * 4) + Name;
}
