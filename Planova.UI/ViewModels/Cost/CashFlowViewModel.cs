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

public partial class CashFlowViewModel : ObservableObject
{
    private readonly ICashFlowService _cashFlowService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private ObservableCollection<CashFlowPeriodDto> _periods = new();

    [ObservableProperty]
    private CashFlowPeriodType _selectedPeriodType = CashFlowPeriodType.Monthly;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ISeries[] _series = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _yAxes = Array.Empty<Axis>();

    public CashFlowViewModel(
        ICashFlowService cashFlowService,
        ICurrentProjectService currentProjectService)
    {
        _cashFlowService = cashFlowService;
        _currentProjectService = currentProjectService;

        XAxes = new Axis[]
        {
            new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("MMM yyyy"))
            {
                Name = "Period",
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
            var data = await _cashFlowService.GetCashFlowAsync(projectId.Value, SelectedPeriodType, null);
            Periods = new ObservableCollection<CashFlowPeriodDto>(data);

            var plannedPoints = new List<DateTimePoint>();
            var actualPoints = new List<DateTimePoint>();

            foreach (var p in data)
            {
                plannedPoints.Add(new DateTimePoint(p.PeriodStart, (double)p.CumulativePlanned));
                actualPoints.Add(new DateTimePoint(p.PeriodStart, (double)p.CumulativeActual));
            }

            Series = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Values = plannedPoints,
                    Name = "Cumulative Planned",
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
                    Fill = null,
                    GeometrySize = 4,
                    GeometryStroke = new SolidColorPaint(SKColors.DodgerBlue, 2)
                },
                new LineSeries<DateTimePoint>
                {
                    Values = actualPoints,
                    Name = "Cumulative Actual",
                    Stroke = new SolidColorPaint(SKColors.OrangeRed, 2),
                    Fill = null,
                    GeometrySize = 4,
                    GeometryStroke = new SolidColorPaint(SKColors.OrangeRed, 2)
                }
            };
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TogglePeriodTypeAsync()
    {
        SelectedPeriodType = SelectedPeriodType == CashFlowPeriodType.Weekly
            ? CashFlowPeriodType.Monthly
            : CashFlowPeriodType.Weekly;
        await LoadAsync();
    }
}
