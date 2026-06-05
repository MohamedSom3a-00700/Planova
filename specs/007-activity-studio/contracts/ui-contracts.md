# UI Contracts: Activity Studio

## Tab Registration Protocol

The Activity Studio follows the same multi-tab workspace pattern as BOQ and WBS studios.

### ActivityStudioView.xaml.cs

```csharp
namespace Planova.UI.Views.Activity;

public partial class ActivityStudioView : UserControl
{
    public ActivityStudioView(ActivityStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (ActivityStudioViewModel)DataContext;
        vm.Tabs.Add(new StudioTab("List", serviceProvider.GetRequiredService<ActivityListView>()));
        vm.Tabs.Add(new StudioTab("Gantt", serviceProvider.GetRequiredService<GanttChartView>()));
        vm.Tabs.Add(new StudioTab("Editor", serviceProvider.GetRequiredService<ActivityEditorView>()));
        vm.Tabs.Add(new StudioTab("Relationships", serviceProvider.GetRequiredService<RelationshipEditorView>()));
        vm.Tabs.Add(new StudioTab("Calendars", serviceProvider.GetRequiredService<CalendarManagerView>()));
        vm.Tabs.Add(new StudioTab("Activity Bank", serviceProvider.GetRequiredService<ActivityBankBrowserView>()));
        vm.Tabs.Add(new StudioTab("Generation", serviceProvider.GetRequiredService<WbsGenerationWizardView>()));
        vm.Tabs.Add(new StudioTab("Reports", serviceProvider.GetRequiredService<ScheduleReportView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
```

### ShellViewModel Registration (Replace Placeholder)

Replace existing placeholder in `Planova.UI/ViewModels/ShellViewModel.cs`:

```csharp
// BEFORE (placeholder):
nav.RegisterTarget("activity", "Activity Studio", "CalendarDay24", true, true,
    () => CreateEmptyState("CalendarDay24", "Activity Studio", "Activity management module is coming soon."));

// AFTER (real):
nav.RegisterTarget("activity", "Activity Studio", "CalendarDay24", true, false, () =>
{
    var view = _serviceProvider.GetRequiredService<ActivityStudioView>();
    view.InitializeTabs(_serviceProvider);
    return view;
});
```

## ViewModel Contracts

### ActivityStudioViewModel

```csharp
public partial class ActivityStudioViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<StudioTab> _tabs = [];

    [ObservableProperty]
    private StudioTab? _selectedTab;
}
```

### ActivityListViewModel

```csharp
public partial class ActivityListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ActivityDto> _activities = [];

    [ObservableProperty]
    private ActivityDto? _selectedActivity;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _statusFilter;

    [ObservableProperty]
    private string? _typeFilter;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadActivitiesAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task DeleteActivityAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task NavigateToEditorAsync(CancellationToken ct) { }
}
```

### GanttChartViewModel

```csharp
public partial class GanttChartViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ActivityDto> _activities = [];

    [ObservableProperty]
    private List<ActivityRelationshipDto> _relationships = [];

    [ObservableProperty]
    private DateTime _viewStart;

    [ObservableProperty]
    private DateTime _viewEnd;

    [ObservableProperty]
    private string _zoomLevel = "Week"; // Day, Week, Month

    [ObservableProperty]
    private double _timeScale; // pixels per day (computed from zoom)

    [RelayCommand]
    private void ZoomIn() { }

    [RelayCommand]
    private void ZoomOut() { }

    [RelayCommand]
    private async Task LoadDataAsync(CancellationToken ct) { }
}
```

### RelationshipEditorViewModel

```csharp
public partial class RelationshipEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ActivityRelationshipDto> _relationships = [];

    [ObservableProperty]
    private ActivityRelationshipDto? _selectedRelationship;

    [ObservableProperty]
    private List<ActivityDto> _availableActivities = [];

    [ObservableProperty]
    private Guid? _selectedPredecessorId;

    [ObservableProperty]
    private Guid? _selectedSuccessorId;

    [ObservableProperty]
    private string _selectedType = "FS";

    [ObservableProperty]
    private int _lagDays;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task CreateRelationshipAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task DeleteRelationshipAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task ValidateRelationshipAsync(CancellationToken ct) { }
}
```

### CalendarManagerViewModel

```csharp
public partial class CalendarManagerViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CalendarDto> _calendars = [];

    [ObservableProperty]
    private CalendarDto? _selectedCalendar;

    [ObservableProperty]
    private ObservableCollection<CalendarDayDto> _calendarDays = [];

    [ObservableProperty]
    private DateTime _currentMonth;

    [RelayCommand]
    private async Task CreateCalendarAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task ToggleDayStatusAsync(DateTime date, CancellationToken ct) { }

    [RelayCommand]
    private async Task BulkSetNonWorkingAsync(DateTime from, DateTime to, CancellationToken ct) { }
}
```

### ActivityBankBrowserViewModel

```csharp
public partial class ActivityBankBrowserViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _categories = [];

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<ActivityBankDto> _entries = [];

    [ObservableProperty]
    private ActivityBankDto? _selectedEntry;

    [ObservableProperty]
    private ActivityBankDto? _previewEntry;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task BrowseCategoryAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task PreviewEntryAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task ApplyToWbsAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task SaveAsCustomEntryAsync(CancellationToken ct) { }
}
```

### WbsGenerationWizardViewModel

```csharp
public partial class WbsGenerationWizardViewModel : ObservableObject
{
    [ObservableProperty]
    private int _currentStep; // 0 = Select WBS, 1 = Choose Mode, 2 = Preview, 3 = Commit

    [ObservableProperty]
    private string _generationMode = "Simple"; // "Simple" or "Bank"

    [ObservableProperty]
    private List<WbsItemDto> _selectedWbsItems = [];

    [ObservableProperty]
    private Guid? _selectedBankId;

    [ObservableProperty]
    private WbsGenerationPreviewDto? _preview;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private int _progressPercentage;

    [ObservableProperty]
    private string _progressMessage = string.Empty;

    [RelayCommand]
    private async Task NextStepAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task CommitGenerationAsync(CancellationToken ct) { }
}
```

### ScheduleReportViewModel

```csharp
public partial class ScheduleReportViewModel : ObservableObject
{
    [ObservableProperty]
    private ScheduleReportDto? _report;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasData;

    [RelayCommand]
    private async Task GenerateReportAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task ExportToExcelAsync(CancellationToken ct) { }

    [RelayCommand]
    private async Task ExportToPdfAsync(CancellationToken ct) { }
}
```
