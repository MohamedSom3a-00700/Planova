using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceHistogramViewModel : ObservableObject
{
    private readonly IResourceHistogramService _histogramService;
    private readonly IResourceService _resourceService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private int _projectId;

    [ObservableProperty]
    private ResourceHistogramDto? _histogramData;

    [ObservableProperty]
    private HistogramFilter _filter = new();

    [ObservableProperty]
    private ObservableCollection<ResourceDto> _availableResources = new();

    [ObservableProperty]
    private ResourceDto? _selectedResource;

    public ResourceHistogramViewModel(
        IResourceHistogramService histogramService,
        IResourceService resourceService,
        ICurrentProjectService currentProjectService)
    {
        _histogramService = histogramService;
        _resourceService = resourceService;
        _currentProjectService = currentProjectService;
        _currentProjectService.CurrentProjectChanged += OnProjectChanged;
        SetProjectContext();
    }

    partial void OnSelectedResourceChanged(ResourceDto? value)
    {
        if (value is not null && ProjectId > 0)
        {
            Filter = Filter with { ResourceId = value.Id };
            _ = LoadHistogramAsync();
        }
    }

    private void OnProjectChanged(object? sender, ProjectContext? project)
    {
        SetProjectContext();
    }

    private async void SetProjectContext()
    {
        var project = _currentProjectService.CurrentProject;
        ProjectId = project?.Id ?? 0;

        if (ProjectId > 0)
        {
            var filter = new ResourceFilter { ProjectId = ProjectId };
            AvailableResources = new ObservableCollection<ResourceDto>(
                await _resourceService.SearchAsync(filter));
        }
        else
        {
            AvailableResources.Clear();
        }

        HistogramData = null;
    }

    [RelayCommand]
    private async Task LoadHistogramAsync()
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
