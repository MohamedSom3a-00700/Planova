using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class EvmViewModel : ObservableObject
{
    private readonly IEvmService _evmService;
    private readonly ICostBaselineRepository _baselineRepository;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private EvmMetricsDto? _metrics;

    [ObservableProperty]
    private ObservableCollection<ActivityEvmDto> _activityDetails = new();

    [ObservableProperty]
    private DateTime _dataDate = DateTime.Today;

    [ObservableProperty]
    private bool _hasBaseline;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _warningMessage = string.Empty;

    [ObservableProperty]
    private ISeries[] _series = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _yAxes = Array.Empty<Axis>();

    public EvmViewModel(
        IEvmService evmService,
        ICostBaselineRepository baselineRepository,
        ICurrentProjectService currentProjectService)
    {
        _evmService = evmService;
        _baselineRepository = baselineRepository;
        _currentProjectService = currentProjectService;

        XAxes = new Axis[]
        {
            new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("MMM yyyy"))
            {
                Name = "Time",
                NameTextSize = 12,
                TextSize = 10,
                LabelsRotation = 45
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "Cost",
                NameTextSize = 12,
                TextSize = 10,
                Labeler = value => value.ToString("N0")
            }
        };
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            var baseline = await _baselineRepository.GetActiveBaselineAsync(projectId.Value);
            HasBaseline = baseline != null;
            WarningMessage = HasBaseline ? string.Empty : "No active baseline. Set a baseline to enable EVM computation.";

            if (HasBaseline)
            {
                Metrics = await _evmService.ComputeMetricsAsync(projectId.Value, DataDate);
                var details = await _evmService.GetActivityDetailAsync(projectId.Value, DataDate);
                ActivityDetails = new ObservableCollection<ActivityEvmDto>(details);
                await LoadChartDataAsync(projectId.Value);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadChartDataAsync(int projectId)
    {
        var baseline = await _baselineRepository.GetActiveBaselineAsync(projectId);
        if (baseline?.Rows == null || baseline.Rows.Count == 0) return;

        var earliestStart = baseline.Rows.Min(r => r.PlannedStart);
        var latestFinish = baseline.Rows.Max(r => r.PlannedFinish);
        var totalDays = (latestFinish - earliestStart).TotalDays;
        if (totalDays <= 0) totalDays = 365;
        var interval = Math.Max(1, (int)(totalDays / 24));

        var pvPoints = new List<DateTimePoint>();
        var evPoints = new List<DateTimePoint>();
        var acPoints = new List<DateTimePoint>();

        for (var d = earliestStart; d <= latestFinish; d = d.AddDays(interval))
        {
            var metrics = await _evmService.ComputeMetricsAsync(projectId, d);
            pvPoints.Add(new DateTimePoint(d, (double)metrics.PlannedValue));
            evPoints.Add(new DateTimePoint(d, (double)metrics.EarnedValue));
            acPoints.Add(new DateTimePoint(d, (double)metrics.ActualCost));
        }

        Series = new ISeries[]
        {
            new LineSeries<DateTimePoint>
            {
                Values = pvPoints,
                Name = "PV (Planned Value)",
                Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
                Fill = null,
                GeometrySize = 3,
                GeometryStroke = new SolidColorPaint(SKColors.DodgerBlue, 1)
            },
            new LineSeries<DateTimePoint>
            {
                Values = evPoints,
                Name = "EV (Earned Value)",
                Stroke = new SolidColorPaint(SKColors.ForestGreen, 2),
                Fill = null,
                GeometrySize = 3,
                GeometryStroke = new SolidColorPaint(SKColors.ForestGreen, 1)
            },
            new LineSeries<DateTimePoint>
            {
                Values = acPoints,
                Name = "AC (Actual Cost)",
                Stroke = new SolidColorPaint(SKColors.OrangeRed, 2),
                Fill = null,
                GeometrySize = 3,
                GeometryStroke = new SolidColorPaint(SKColors.OrangeRed, 1)
            }
        };
    }
}
