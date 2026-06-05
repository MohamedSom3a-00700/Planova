# Quickstart — Resource Studio

## Overview

Resource Studio (Phase 6) adds construction resource management to Planova. It manages labour, equipment, material, and subcontractor resources with effective-dated rates, reusable crew templates, resource loading onto activities, resource histograms, and AI-powered estimation.

## Architecture

```
Planova.Resource/          ← New module (Class Library)
├── Domain/                ← Entities, Enums, Interfaces
├── Application/           ← Services, DTOs, Mappings
├── Extensions/            ← DI registration

Planova.Persistence/       ← Existing, add configurations + repos
Planova.UI/                ← Existing, add Views + ViewModels
Planova.Localization/      ← Existing, add resx files
Planova.Excel/             ← Existing, add readers/writers

tests/Planova.Resource.Tests/  ← New test project
```

## Prerequisites

- .NET 8 SDK
- Planova solution built with all existing modules (Planova.Domain, Planova.Shared, Planova.Persistence, Planova.UI)
- Phase 5 (Activity Studio) completed — Resource Studio depends on Activity entities for assignments
- Existing PlanovaDbContext with EF Core migrations infrastructure

## Setup Steps

### 1. Create the Resource Module Project

```bash
dotnet new classlib -n Planova.Resource -o Planova.Resource --framework net8.0
dotnet sln Planova.slnx add Planova.Resource/Planova.Resource.csproj
dotnet add Planova.Resource/Planova.Resource.csproj reference Planova.Domain/Planova.Domain.csproj
dotnet add Planova.Resource/Planova.Resource.csproj reference Planova.Shared/Planova.Shared.csproj
dotnet add Planova.Resource/Planova.Resource.csproj reference Planova.Activity/Planova.Activity.csproj
```

### 2. Create the Test Project

```bash
dotnet new xunit -n Planova.Resource.Tests -o tests/Planova.Resource.Tests --framework net8.0
dotnet sln Planova.slnx add tests/Planova.Resource.Tests/Planova.Resource.Tests.csproj
dotnet add tests/Planova.Resource.Tests/Planova.Resource.Tests.csproj reference Planova.Resource/Planova.Resource.csproj
```

### 3. Add Entities and Enums

Create folder structure under `Planova.Resource/Domain/`:

```
Domain/
├── Entities/
│   ├── Resource.cs
│   ├── ResourceRate.cs
│   ├── Crew.cs
│   ├── CrewResource.cs
│   ├── ResourceAssignment.cs
│   └── ResourceUsage.cs
├── Enums/
│   ├── ResourceType.cs
│   ├── ResourceScope.cs
│   ├── ResourceStatus.cs
│   ├── CrewStatus.cs
│   └── HistogramAggregation.cs
└── Interfaces/
    ├── IResourceRepository.cs
    ├── IResourceRateRepository.cs
    ├── ICrewRepository.cs
    ├── ICrewResourceRepository.cs
    ├── IResourceAssignmentRepository.cs
    ├── IResourceUsageRepository.cs
    ├── IResourceService.cs
    ├── ICrewService.cs
    ├── IResourceAssignmentService.cs
    ├── IResourceHistogramService.cs
    ├── IResourceAiEstimationService.cs
    ├── IResourceReportService.cs
    └── IResourceImportService.cs
```

### 4. Add Application Services and DTOs

Create folder structure under `Planova.Resource/Application/`:

```
Application/
├── Services/
│   ├── ResourceService.cs
│   ├── ResourceRateService.cs
│   ├── CrewService.cs
│   ├── ResourceAssignmentService.cs
│   ├── ResourceHistogramService.cs
│   ├── ResourceAiEstimationService.cs
│   ├── ResourceReportService.cs
│   └── ResourceImportService.cs
├── Dto/
│   ├── ResourceDto.cs
│   ├── ResourceRateDto.cs
│   ├── CrewDto.cs
│   ├── CrewResourceDto.cs
│   ├── ResourceAssignmentDto.cs
│   ├── ResourceHistogramDto.cs
│   ├── AiSuggestionDto.cs
│   ├── ResourceReportDto.cs
│   └── ImportResultDto.cs
└── Mappings/
    └── ResourceMappingProfile.cs
```

### 5. Register DI

```csharp
// Planova.Resource/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddPlanovaResource(this IServiceCollection services)
{
    services.AddScoped<IResourceService, ResourceService>();
    services.AddScoped<ICrewService, CrewService>();
    services.AddScoped<IResourceAssignmentService, ResourceAssignmentService>();
    services.AddScoped<IResourceHistogramService, ResourceHistogramService>();
    services.AddScoped<IResourceAiEstimationService, ResourceAiEstimationService>();
    services.AddScoped<IResourceReportService, ResourceReportService>();
    services.AddScoped<IResourceImportService, ResourceImportService>();
    return services;
}
```

Then call `services.AddPlanovaResource()` in `App.xaml.cs` alongside `AddPlanovaActivity()` and others.

### 6. Add Persistence

Add to `Planova.Persistence`:

- Entity configurations: `ResourceConfiguration.cs`, `ResourceRateConfiguration.cs`, `CrewConfiguration.cs`, `CrewResourceConfiguration.cs`, `ResourceAssignmentConfiguration.cs`, `ResourceUsageConfiguration.cs`
- Repository implementations: `ResourceRepository.cs`, `ResourceRateRepository.cs`, `CrewRepository.cs`, `CrewResourceRepository.cs`, `ResourceAssignmentRepository.cs`, `ResourceUsageRepository.cs`
- Register repositories in `ServiceCollectionExtensions.cs`
- Apply configurations in `PlanovaDbContext.OnModelCreating`
- Create EF Core migration: `dotnet ef migrations add AddResourceEntities`

### 7. Add UI Components

Add to `Planova.UI`:

- `ViewModels/Resource/` — Studio, Library, Editor, Rate Manager, Crew Manager, Crew Editor, Assignment, Histogram, AI, Report
- `Views/Resource/` — Corresponding XAML views
- Register Resource Studio as a navigation target in `ShellViewModel`

### 8. Add Localization Resources

Add to `Planova.Localization/Resources/`:

- `ResourceResources.en.resx` — English strings
- `ResourceResources.ar.resx` — Arabic strings (RTL)

### 9. Add Excel Import/Export

Add to `Planova.Excel`:

- `Readers/ResourceImportReader.cs`
- `Writers/ResourceReportWriter.cs`

## Key Workflows

### Managing Resources

1. Open Resource Studio → Browse library tab
2. Filter by type (Labour/Equipment/Material/Subcontractor) or search by name/code
3. Create new resource → select type → fill type-specific fields → save (auto-generates code)
4. Edit existing resource → modify fields → save (with duplicate name warning if applicable)
5. Deactivate resource → soft delete (preserves historical assignments)

### Managing Rates

1. Select a resource → open Rate Manager
2. View rate history (chronological with effective dates)
3. Add new rate → set effective date, value, currency → save
4. Future-dated rates become active automatically on their effective date
5. Rate resolution uses latest effective date on or before the query date

### Managing Crew Templates

1. Open Crew Templates tab → create new crew
2. Name the crew → add resources from the library with quantities and lead flag
3. View blended rate (auto-calculated sum of Quantity × Rate)
4. Apply crew to one or more activities → individual assignments created per crew member
5. Clone a crew template to create variations

### Loading Resources onto Activities

1. Open Activity Studio → select an activity
2. Open Resource Assignment panel → assign a resource with quantity and rate
3. Or apply a crew template for bulk assignment
4. View total cost (auto-calculated: Quantity × Rate × Duration)
5. Edit/remove assignments with real-time cost updates

### Viewing Histogram

1. Open Histogram tab → view daily resource usage across project timeline
2. Filter by resource type, specific resource, time range
3. Overallocation highlighted when assigned quantity exceeds available quantity
4. Export histogram data to spreadsheet

### AI Estimation

1. Select an activity → click "Estimate Resources"
2. AI analyzes activity name, description, and WBS category
3. Suggested resources displayed with quantities and confidence scores
4. Accept all, adjust quantities, or reject
5. Graceful fallback if AI provider unavailable

### Reports

1. Open Reports tab → select report type (Usage Summary or Cost Report)
2. Generate report → view on screen
3. Export to Excel (ClosedXML) or PDF (QuestPDF)
4. Print preview available

## Testing

```bash
# Run Resource Studio tests
dotnet test tests/Planova.Resource.Tests/Planova.Resource.Tests.csproj

# Run all tests
dotnet test
```

Test categories:
- **Domain tests**: Entity behavior, validation, state transitions
- **Service tests**: Resource service, crew service, assignment service (mocked repositories)
- **Persistence tests**: Repository implementations against SQLite in-memory
- **UI tests**: ViewModel behavior (if applicable)

## Dependencies

| Dependency | Version | Purpose |
|-----------|---------|---------|
| Planova.Domain | — | Base entities, value objects |
| Planova.Shared | — | Cross-cutting abstractions |
| Planova.Activity | — | Activity entity for resource assignments |
| Semantic Kernel | 1.x | AI estimation abstraction |
| LiveCharts2 | 2.x | Histogram charting |
| QuestPDF | 2024.x | PDF report generation |
| ClosedXML | 0.102.x | Excel import/export |

## Related Documents

- [spec.md](spec.md) — Feature specification
- [plan.md](plan.md) — Implementation plan
- [research.md](research.md) — Design decisions and research
- [data-model.md](data-model.md) — Entity definitions and relationships
- [contracts/](contracts/) — Interface contracts
