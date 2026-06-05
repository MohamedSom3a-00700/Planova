using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceHistogramViewModel : ObservableObject
{
    private readonly IResourceHistogramService _histogramService;

    [ObservableProperty]
    private int _projectId;

    [ObservableProperty]
    private ResourceHistogramDto? _histogramData;

    [ObservableProperty]
    private HistogramFilter _filter = new();

    public ResourceHistogramViewModel(IResourceHistogramService histogramService)
    {
        _histogramService = histogramService;
    }

    [RelayCommand]
    private async Task LoadHistogram()
    {
        if (ProjectId <= 0) return;
        HistogramData = await _histogramService.GetHistogramAsync(ProjectId, Filter);
    }

    [RelayCommand]
    private async Task ExportData()
    {
        if (ProjectId <= 0) return;

        var dialog = new SaveFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            Title = "Export Histogram Data",
            FileName = "histogram.xlsx"
        };
        if (dialog.ShowDialog() != true) return;

        var data = await _histogramService.ExportHistogramDataAsync(ProjectId, Filter);
        await File.WriteAllBytesAsync(dialog.FileName, data);
    }
}
