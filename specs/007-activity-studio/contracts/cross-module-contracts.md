# Cross-Module Contracts: Activity Studio

## WBS Studio Integration

Activity Studio consumes WBS entities for traceability and generation.

### Required WBS Interfaces (read-only)

```csharp
// Accessed from Planova.Wbs.Domain.Interfaces
// These interfaces already exist; Activity Studio calls them as a consumer.

public interface IWbsItemService
{
    Task<List<WbsItemDto>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);
    Task<List<WbsItemDto>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<WbsTreeDto?> GetTreeByProjectIdAsync(int projectId, CancellationToken ct = default);
}

public interface IWbsRepository
{
    Task<Wbs?> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
}
```

### Dependency Graph

```
Planova.Activity
  ├── references Planova.Domain (for WbsItem, Wbs entities)
  ├── references Planova.Shared (for ILocalizationService, etc.)
  └── references Planova.Wbs (for IWbsItemService, IWbsRepository via Application)

Planova.Persistence
  └── references Planova.Activity (for entity configurations)
```

### Query Pattern (Cross-Module)

```csharp
// In ActivityService or WbsGenerationService:
var wbsItems = await _wbsItemService.GetByIdsAsync(request.WbsItemIds, ct);
// Use WbsItem.Name, WbsItem.Code, WbsItem.Level to generate activity names/codes
```

## Localization Integration

```csharp
// Planova.Localization/Resources/
// New files:
//   ActivityResources.en.resx  — English strings for all Activity Studio UI
//   ActivityResources.ar.resx  — Arabic translations

// Usage in ViewModels:
var localizedName = _localizationService.GetString("ActivityStudio_Title");
```

## Report Export Integration

```csharp
// Excel: uses existing Planova.Excel.IWorkbookWriter
var workbook = _workbookWriter.CreateWorkbook();
var sheet = workbook.AddSheet("Schedule Report");
// ... populate sheet from ScheduleReportDto ...

// PDF: uses QuestPDF directly in ActivityReportService
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Element(compose => BuildReportContent(compose, report));
    });
}).GeneratePdf();
```

## Project Module Integration

```csharp
// Activity belongs to a Project (ProjectId FK)
// Project entity already exists in Planova.Domain
// No new Project interfaces needed — ProjectId is an int FK
```
