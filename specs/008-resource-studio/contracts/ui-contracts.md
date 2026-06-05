# UI Contracts — Resource Studio

## ViewModel Contracts

Each ViewModel follows the established MVVM pattern using CommunityToolkit.Mvvm.

### ResourceStudioViewModel

Main workspace view model that manages sub-tabs for Resource Studio features.

```csharp
public partial class ResourceStudioViewModel : ObservableObject
{
    // Navigation tabs managed as inner workspace tabs
    // Registered as navigation target in ShellViewModel

    [RelayCommand]
    private async Task OpenResourceLibrary();

    [RelayCommand]
    private async Task OpenCrewTemplates();

    [RelayCommand]
    private async Task OpenHistogram();

    [RelayCommand]
    private async Task OpenReports();
}
```

### ResourceLibraryViewModel

Lists, filters, and searches resources.

```csharp
public partial class ResourceLibraryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ResourceDto> _resources;

    [ObservableProperty]
    private ResourceFilter _filter;

    [ObservableProperty]
    private ResourceDto? _selectedResource;

    [RelayCommand]
    private async Task LoadResources();

    [RelayCommand]
    private async Task Search(string query);

    [RelayCommand]
    private async Task FilterByType(ResourceType? type);

    [RelayCommand]
    private async Task CreateResource();

    [RelayCommand]
    private async Task EditResource(ResourceDto resource);

    [RelayCommand]
    private async Task DeactivateResource(ResourceDto resource);

    [RelayCommand]
    private async Task ReactivateResource(ResourceDto resource);

    [RelayCommand]
    private async Task ImportResources();

    [RelayCommand]
    private async Task ExportResources();
}
```

### ResourceEditorViewModel

Creates or edits a single resource.

```csharp
public partial class ResourceEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private ResourceDto? _resource;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private string _title;

    // Type-specific dynamic fields visibility
    [ObservableProperty]
    private bool _showLabourFields;

    [ObservableProperty]
    private bool _showEquipmentFields;

    [ObservableProperty]
    private bool _showMaterialFields;

    [ObservableProperty]
    private bool _showSubcontractorFields;

    public ObservableCollection<ResourceRateDto> Rates { get; }

    [RelayCommand]
    private async Task Save();

    [RelayCommand]
    private async Task Cancel();

    [RelayCommand]
    private async Task AddRate();

    [RelayCommand]
    private async Task RemoveRate(ResourceRateDto rate);
}
```

### ResourceRateManagerViewModel

Manages effective-dated rates for a resource.

```csharp
public partial class ResourceRateManagerViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _resourceId;

    [ObservableProperty]
    private ObservableCollection<ResourceRateDto> _rates;

    [ObservableProperty]
    private ResourceRateDto? _selectedRate;

    [RelayCommand]
    private async Task LoadRateHistory();

    [RelayCommand]
    private async Task AddRate(CreateRateRequest request);

    [RelayCommand]
    private async Task RemoveRate(ResourceRateDto rate);

    [RelayCommand]
    private async Task BulkRateUpdate();
}
```

### CrewTemplateManagerViewModel

Lists and manages crew templates.

```csharp
public partial class CrewTemplateManagerViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CrewDto> _crews;

    [ObservableProperty]
    private CrewDto? _selectedCrew;

    [RelayCommand]
    private async Task LoadCrews();

    [RelayCommand]
    private async Task CreateCrew();

    [RelayCommand]
    private async Task EditCrew(CrewDto crew);

    [RelayCommand]
    private async Task CloneCrew(CrewDto crew);

    [RelayCommand]
    private async Task DeleteCrew(CrewDto crew);

    [RelayCommand]
    private async Task ApplyCrewToActivities(CrewDto crew);
}
```

### CrewTemplateEditorViewModel

Edits a single crew template composition.

```csharp
public partial class CrewTemplateEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private CrewDto _crew;

    [ObservableProperty]
    private decimal _blendedRate;

    [ObservableProperty]
    private ObservableCollection<CrewResourceDto> _resources;

    [RelayCommand]
    private async Task AddResource(CrewResourceInput input);

    [RelayCommand]
    private async Task RemoveResource(CrewResourceDto resource);

    [RelayCommand]
    private async Task UpdateQuantity(CrewResourceDto resource, decimal quantity);

    [RelayCommand]
    private async Task Save();

    [RelayCommand]
    private async Task Cancel();
}
```

### ResourceAssignmentViewModel

Assigns resources to Phase 5 activities.

```csharp
public partial class ResourceAssignmentViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private ObservableCollection<ResourceAssignmentDto> _assignments;

    [ObservableProperty]
    private decimal _totalCost;

    [RelayCommand]
    private async Task LoadAssignments();

    [RelayCommand]
    private async Task AddAssignment(CreateAssignmentRequest request);

    [RelayCommand]
    private async Task EditAssignment(UpdateAssignmentRequest request);

    [RelayCommand]
    private async Task RemoveAssignment(ResourceAssignmentDto assignment);

    [RelayCommand]
    private async Task ApplyCrew(CrewDto crew);
}
```

### ResourceHistogramViewModel

Renders resource histogram chart.

```csharp
public partial class ResourceHistogramViewModel : ObservableObject
{
    [ObservableProperty]
    private ResourceHistogramDto _histogramData;

    [ObservableProperty]
    private HistogramFilter _filter;

    // LiveCharts2 chart series
    [ObservableProperty]
    private SeriesCollection _chartSeries;

    [ObservableProperty]
    private Axis[] _xAxes;

    [ObservableProperty]
    private Axis[] _yAxes;

    [RelayCommand]
    private async Task LoadHistogram();

    [RelayCommand]
    private async Task FilterByType(ResourceType? type);

    [RelayCommand]
    private async Task FilterByResource(Guid? resourceId);

    [RelayCommand]
    private async Task FilterByDateRange(DateTime? from, DateTime? to);

    [RelayCommand]
    private async Task ExportData();
}
```

### ResourceAiEstimationViewModel

AI-powered resource suggestion interface.

```csharp
public partial class ResourceAiEstimationViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private bool _isEstimating;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private ObservableCollection<AiSuggestionDto> _suggestions;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task EstimateResources();

    [RelayCommand]
    private async Task AcceptAll();

    [RelayCommand]
    private async Task AcceptAdjusted(AcceptedSuggestionDto adjusted);

    [RelayCommand]
    private async Task RejectAll();

    [RelayCommand]
    private async Task AdjustQuantity(AiSuggestionDto suggestion, decimal newQuantity);
}
```

### ResourceReportViewModel

Generates and exports resource reports.

```csharp
public partial class ResourceReportViewModel : ObservableObject
{
    [ObservableProperty]
    private ReportType _selectedReportType;

    [ObservableProperty]
    private object? _reportData; // UsageSummary or CostReport

    [ObservableProperty]
    private bool _isGenerating;

    [RelayCommand]
    private async Task GenerateReport();

    [RelayCommand]
    private async Task ExportToExcel();

    [RelayCommand]
    private async Task ExportToPdf();

    [RelayCommand]
    private async Task PrintPreview();
}
```
