# UI Contracts — Reporting Center

## ViewModel Interfaces

### IReportingHubViewModel

```csharp
public interface IReportingHubViewModel
{
    bool HasActiveProject { get; }
    Task LoadAsync(int projectId, CancellationToken ct = default);
}
```

### IReportTabViewModel

```csharp
public interface IReportTabViewModel
{
    ReportType ReportType { get; }
    DateTime? SelectedPeriodStart { get; set; }
    DateTime? SelectedPeriodEnd { get; set; }
    bool HasData { get; }
    bool IsGenerating { get; }
    bool IsAiAvailable { get; }
    string? AiNarrative { get; set; }
    ICommand GenerateReportCommand { get; }
    ICommand GenerateAiNarrativeCommand { get; }
    ICommand RegenerateAiNarrativeCommand { get; }
    ICommand ExportToExcelCommand { get; }
    ICommand ExportToPdfCommand { get; }
    ICommand ExportToWordCommand { get; }
}
```

## Navigation Contracts

### Shell Integration

The existing `"reports"` nav target must be updated:

| Property | Current | After Phase 8 |
|----------|---------|---------------|
| Target ID | `"reports"` | `"reports"` (unchanged) |
| Display Name | `"Reports"` | `"Reports"` (unchanged) |
| Icon | `DocumentText24` | `DocumentText24` (unchanged) |
| `isStudio` | `false` | `true` |
| View Factory | `ReportView` | `ReportingHubView` |
| Studio Target IDs | Not in list | Added to `_studioTargetIds` |

### DI Registration

```csharp
// Module registration
services.AddPlanovaReporting();

// ViewModels
services.AddTransient<ReportingHubViewModel>();
services.AddTransient<DailyReportViewModel>();
services.AddTransient<WeeklyReportViewModel>();
services.AddTransient<MonthlyReportViewModel>();
services.AddTransient<ExecutiveReportViewModel>();
services.AddTransient<ReportScheduleViewModel>();
services.AddTransient<ReportHistoryViewModel>();
services.AddTransient<ReportTemplateEditorViewModel>();
services.AddTransient<ReportSettingsViewModel>();
services.AddTransient<ProjectPartyViewModel>();

// Views
services.AddTransient<ReportingHubView>();
services.AddTransient<DailyReportView>();
services.AddTransient<WeeklyReportView>();
services.AddTransient<MonthlyReportView>();
services.AddTransient<ExecutiveReportView>();
services.AddTransient<ReportScheduleView>();
services.AddTransient<ReportHistoryView>();
services.AddTransient<ReportTemplateEditorView>();
services.AddTransient<ReportSettingsView>();
```

The old `ReportView`/`ReportViewModel` registrations are removed.
