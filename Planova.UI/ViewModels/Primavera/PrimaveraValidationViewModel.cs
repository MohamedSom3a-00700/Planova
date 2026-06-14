using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class DcmaThresholdConfig : ObservableObject
{
    [ObservableProperty]
    private int _pointNumber;

    [ObservableProperty]
    private string _pointName = string.Empty;

    [ObservableProperty]
    private int? _minNumber;

    [ObservableProperty]
    private double? _minPercentage;

    [ObservableProperty]
    private DcmaStatus _status;

    [ObservableProperty]
    private int _issueCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private double _percentage;

    [ObservableProperty]
    private string _details = string.Empty;
}

public partial class PrimaveraValidationViewModel : ObservableObject
{
    private readonly IPrimaveraValidationService _validationService;
    private readonly IPrimaveraImportService _importService;
    private List<PrimaveraValidationIssueDto> _allIssues = new();
    private DcmaAssessmentResultDto? _rawDcmaResult;

    public PrimaveraValidationViewModel(
        IPrimaveraValidationService validationService,
        IPrimaveraImportService importService)
    {
        _validationService = validationService;
        _importService = importService;
        _ = LoadImportedSessionsAsync();
    }

    public ObservableCollection<PrimaveraValidationIssueDto> Issues { get; } = new();

    public ObservableCollection<GroupedIssuesViewModel> GroupedIssues { get; } = new();

    public ObservableCollection<XerImportSessionDto> ImportedSessions { get; } = new();

    public ObservableCollection<DcmaThresholdConfig> DcmaThresholds { get; } = new();

    [ObservableProperty]
    private XerImportSessionDto? _selectedSession;

    partial void OnSelectedSessionChanged(XerImportSessionDto? value)
    {
        if (value != null)
            StatusMessage = $"Selected: {value.SourceFileName} ({value.ImportedAt:g})";
    }

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _selectedSeverityFilter = "All Issues";

    [ObservableProperty]
    private bool _isDcmaRunning;

    [ObservableProperty]
    private DcmaAssessmentResultDto? _dcmaResult;

    [ObservableProperty]
    private double _overallScore;

    [ObservableProperty]
    private string _overallScoreColor = "Gray";

    partial void OnSelectedSeverityFilterChanged(string value)
    {
        ApplyGrouping();
    }

    public async Task LoadImportedSessionsAsync()
    {
        try
        {
            var sessions = await _importService.GetImportedSessionsAsync();
            ImportedSessions.Clear();
            foreach (var s in sessions)
                ImportedSessions.Add(s);
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task RunValidationAsync()
    {
        IsValidating = true;
        StatusMessage = "Running validation...";
        Issues.Clear();

        try
        {
            var results = await _validationService.ValidateAsync(1);
            _allIssues = results;
            foreach (var issue in results)
                Issues.Add(issue);

            ApplyGrouping();

            StatusMessage = results.Count == 0
                ? "No issues found."
                : $"Found {results.Count} issue(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Validation failed: {ex.Message}";
        }
        finally
        {
            IsValidating = false;
        }
    }

    [RelayCommand]
    private async Task RunDcmaAssessmentAsync()
    {
        IsDcmaRunning = true;
        StatusMessage = "Running DCMA 14-point assessment...";

        try
        {
            _rawDcmaResult = await _validationService.AssessDcma14PointAsync(1);
            DcmaResult = _rawDcmaResult;
            OverallScore = DcmaResult.OverallScore;
            OverallScoreColor = OverallScore >= 80 ? "#28A745" : OverallScore >= 50 ? "#FFB83B" : "#E81123";

            InitializeThresholds();
            EvaluateThresholds();

            StatusMessage = $"DCMA assessment complete. Score: {DcmaResult.OverallScore}%";
        }
        catch (Exception ex)
        {
            StatusMessage = $"DCMA assessment failed: {ex.Message}";
        }
        finally
        {
            IsDcmaRunning = false;
        }
    }

    private void InitializeThresholds()
    {
        DcmaThresholds.Clear();
        if (_rawDcmaResult == null) return;

        foreach (var point in _rawDcmaResult.Points)
        {
            int? defaultMin = point.PointNumber switch
            {
                1 => null,
                2 => 0,
                3 => null,
                4 => 3,
                5 => null,
                6 => 0,
                7 => 0,
                8 => 5,
                9 => 0,
                10 => 3,
                11 => null,
                12 => 5,
                13 => 0,
                14 => 0,
                _ => null
            };

            double? defaultPct = point.PointNumber switch
            {
                1 => 10.0,
                3 => 5.0,
                5 => 10.0,
                8 => null,
                10 => null,
                11 => 20.0,
                12 => null,
                _ => null
            };

            DcmaThresholds.Add(new DcmaThresholdConfig
            {
                PointNumber = point.PointNumber,
                PointName = point.Name,
                MinNumber = defaultMin,
                MinPercentage = defaultPct,
                Status = point.Status,
                IssueCount = point.IssueCount,
                TotalCount = point.TotalCount,
                Percentage = point.Percentage,
                Details = point.Details
            });
        }
    }

    private bool _isReevaluating;

    [RelayCommand]
    private void ReevaluateThresholds()
    {
        if (DcmaThresholds.Count == 0) return;
        _isReevaluating = true;
        EvaluateThresholds();
        _isReevaluating = false;
    }

    private void EvaluateThresholds()
    {
        if (_rawDcmaResult == null) return;

        int passed = 0;

        foreach (var threshold in DcmaThresholds)
        {
            var point = _rawDcmaResult.Points.FirstOrDefault(p => p.PointNumber == threshold.PointNumber);
            if (point == null) continue;

            threshold.IssueCount = point.IssueCount;
            threshold.TotalCount = point.TotalCount;
            threshold.Percentage = point.Percentage;

            bool failsByNumber = threshold.MinNumber.HasValue && point.IssueCount > threshold.MinNumber.Value;
            bool failsByPercent = threshold.MinPercentage.HasValue &&
                point.IssueCount > 0 &&
                point.Percentage > threshold.MinPercentage.Value;

            if (failsByNumber || failsByPercent)
                threshold.Status = DcmaStatus.Fail;
            else if (point.IssueCount == 0)
                threshold.Status = DcmaStatus.Pass;
            else
                threshold.Status = DcmaStatus.Warning;

            if (threshold.Status == DcmaStatus.Pass)
                passed++;
        }

        if (_isReevaluating)
        {
            OverallScore = Math.Round(passed / (double)DcmaThresholds.Count * 100, 1);
            OverallScoreColor = OverallScore >= 80 ? "#28A745" : OverallScore >= 50 ? "#FFB83B" : "#E81123";
        }
    }

    public static string GetStatusColor(DcmaStatus status)
    {
        return status switch
        {
            DcmaStatus.Pass => "#28A745",
            DcmaStatus.Warning => "#FFB83B",
            DcmaStatus.Fail => "#E81123",
            _ => "Gray"
        };
    }

    private void ApplyGrouping()
    {
        GroupedIssues.Clear();
        var filtered = SelectedSeverityFilter switch
        {
            "Errors" => _allIssues.Where(i => i.Severity == "Error").ToList(),
            "Warnings" => _allIssues.Where(i => i.Severity == "Warning").ToList(),
            "Info" => _allIssues.Where(i => i.Severity == "Info").ToList(),
            _ => _allIssues
        };

        foreach (var group in filtered.GroupBy(i => i.Severity))
        {
            GroupedIssues.Add(new GroupedIssuesViewModel
            {
                Key = group.Key,
                Items = new ObservableCollection<PrimaveraValidationIssueDto>(group)
            });
        }
    }
}

public class GroupedIssuesViewModel
{
    public string Key { get; set; } = string.Empty;
    public ObservableCollection<PrimaveraValidationIssueDto> Items { get; set; } = new();
}