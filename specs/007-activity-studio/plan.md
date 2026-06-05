# Implementation Plan: Activity Studio

**Branch**: `007-activity-studio` | **Date**: 2026-06-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/007-activity-studio/spec.md`

## Summary

Implement the Activity Studio module (Phase 5) — transforming WBS work packages into a detailed project schedule with activities, logic relationships, milestones, calendars, and a reusable Activity Bank of pre-defined construction method sequences. Users create/edit/delete schedule activities, view them on a custom Canvas-based Gantt chart with dependency arrows, define FS/SS/FF/SF relationships with circular reference detection, manage working time calendars (Global & Project), generate activities from WBS (1:1 or via Activity Bank templates), and export schedule reports to Excel and PDF. Built on .NET 8 + WPF (WPF-UI), following Clean Architecture and MVVM.

## Technical Context

**Language/Version**: .NET 8, C# 12, net8.0-windows

**Primary Dependencies**: CommunityToolkit.Mvvm, WPF-UI (v4.3.0), EF Core, SQLite, Serilog, ClosedXML (Excel export), QuestPDF (PDF export), Microsoft.Extensions.Hosting, Microsoft.Extensions.DependencyInjection

**Storage**: SQLite via EF Core. New entities: Activity, ActivityRelationship, Calendar, CalendarDay, ActivityBank, ActivityBankItem, ActivityBankItemRelationship. Existing Wbs/WbsItem tables consumed (read-only) for WBS traceability.

**Testing**: xUnit — unit tests for domain entities, validation, application services, relationship circular reference detection, calendar date calculations, WBS generation logic, Activity Bank apply/merge. Moq for mocking. FluentAssertions for assertions.

**Target Platform**: Windows (WPF Desktop, net8.0-windows)

**Project Type**: Desktop application module (new "Activity Studio" module per Modular Domain Design)

**Performance Goals**: Gantt render 1,000 activities < 2s, circular reference detection on 10k activities < 1s, bank entry apply to 20 WBS items generating 200+ activities < 10s, schedule report for 500+ activities < 15s

**Constraints**: Clean Architecture, MVVM, async-first with CancellationToken, large dataset virtualization (Gantt, activity list), RTL support via FlowDirection, self-referencing parent-child for bank items, custom Canvas-based Gantt using DrawingVisual for performance

**Scale/Scope**: Single-user desktop, up to 100k+ activities per project, 50+ seeded Activity Bank entries across 13 categories

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|-----------|-------|
| I. Architecture First | ✅ PASS | Clean Architecture with Planova.Activity (Domain + Application), Planova.UI for Views/ViewModels, Planova.Persistence for EF configs; no dependency bypass |
| II. MVVM & Fluent UI | ✅ PASS | WPF + MVVM via CommunityToolkit.Mvvm; WPF-UI for Fluent-style controls; navigation rail + multi-tab workspace pattern |
| III. Modular Domain | ✅ PASS | Activity Studio is a recognized module with dedicated Planova.Activity project exposing clear contracts (IActivityService, ICalendarService, IActivityBankService, etc.) |
| IV. Build vs Buy | ✅ PASS | QuestPDF + ClosedXML for reports (approved); custom Canvas Gantt chart is necessary domain-specific visualization — no off-the-shelf Gantt library meets the performance and customization requirements; no workflow engine or automation designer needed |
| V. Automation Agnostic | ✅ PASS | N/A — no automation workflows in scope for Phase 5 |
| VI. AI Provider Agnostic | ✅ PASS | N/A — no AI features in Phase 5 scope |
| VII. Multilingual First | ✅ PASS | English + Arabic runtime switching; ActivityResources.en.resx and .ar.resx files; RTL-aware Gantt and activity list views |
| VIII. Performance & Scalability | ✅ PASS | Async-first (IActivityService, ICalendarService, etc.), CancellationToken throughout, virtualization for large activity lists, virtualized Gantt with visible-range-only rendering, efficient EF queries with indexed FK columns (WbsItemId, CalendarId, ProjectId) |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/007-activity-studio/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Planova.Activity/                                     # NEW — Domain + Application
├── Domain/
│   ├── Entities/
│   │   ├── Activity.cs
│   │   ├── ActivityRelationship.cs
│   │   ├── Calendar.cs
│   │   ├── CalendarDay.cs
│   │   ├── ActivityBank.cs
│   │   ├── ActivityBankItem.cs
│   │   └── ActivityBankItemRelationship.cs
│   ├── Enums/
│   │   ├── ActivityType.cs
│   │   ├── ActivityStatus.cs
│   │   ├── RelationshipType.cs
│   │   └── CalendarType.cs
│   └── Interfaces/
│       ├── IActivityRepository.cs
│       ├── IActivityRelationshipRepository.cs
│       ├── ICalendarRepository.cs
│       ├── IActivityBankRepository.cs
│       ├── IActivityService.cs
│       ├── IActivityRelationshipService.cs
│       ├── ICalendarService.cs
│       ├── IActivityBankService.cs
│       ├── IWbsGenerationService.cs
│       └── IActivityReportService.cs
├── Application/
│   ├── Services/
│   │   ├── ActivityService.cs
│   │   ├── ActivityRelationshipService.cs
│   │   ├── CircularReferenceDetector.cs
│   │   ├── CalendarService.cs
│   │   ├── CalendarDateCalculator.cs
│   │   ├── ActivityBankService.cs
│   │   ├── WbsGenerationService.cs
│   │   └── ActivityReportService.cs
│   └── Dto/
│       ├── ActivityDto.cs
│       ├── ActivityRelationshipDto.cs
│       ├── CalendarDto.cs
│       ├── CalendarDayDto.cs
│       ├── ActivityBankDto.cs
│       ├── ActivityBankItemDto.cs
│       ├── WbsGenerationRequest.cs
│       ├── WbsGenerationPreviewDto.cs
│       ├── ScheduleReportDto.cs
│       └── CircularReferenceCheckResult.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── GlobalUsings.cs
└── Planova.Activity.csproj

Planova.UI/
├── Views/Activity/                                   # NEW
│   ├── ActivityStudioView.xaml / .cs
│   ├── ActivityListView.xaml
│   ├── ActivityEditorView.xaml
│   ├── GanttChartView.xaml
│   ├── RelationshipEditorView.xaml
│   ├── CalendarManagerView.xaml
│   ├── CalendarDayGridView.xaml
│   ├── ActivityBankBrowserView.xaml
│   ├── ActivityBankPreviewView.xaml
│   ├── WbsGenerationWizardView.xaml
│   └── ScheduleReportView.xaml
├── ViewModels/Activity/                              # NEW
│   ├── ActivityStudioViewModel.cs
│   ├── ActivityListViewModel.cs
│   ├── ActivityEditorViewModel.cs
│   ├── GanttChartViewModel.cs
│   ├── RelationshipEditorViewModel.cs
│   ├── CalendarManagerViewModel.cs
│   ├── CalendarDayGridViewModel.cs
│   ├── ActivityBankBrowserViewModel.cs
│   ├── ActivityBankPreviewViewModel.cs
│   ├── WbsGenerationWizardViewModel.cs
│   └── ScheduleReportViewModel.cs
└── Controls/                                         # NEW
    └── ActivityGanttCanvas.cs                        # Custom DrawingVisual-based Gantt

Planova.Persistence/
├── DbContext/                                        # MODIFY — add new DbSets
│   └── PlanovaDbContext.cs
└── EntityConfigurations/                             # NEW
    ├── ActivityConfiguration.cs
    ├── ActivityRelationshipConfiguration.cs
    ├── CalendarConfiguration.cs
    ├── CalendarDayConfiguration.cs
    ├── ActivityBankConfiguration.cs
    ├── ActivityBankItemConfiguration.cs
    └── ActivityBankItemRelationshipConfiguration.cs

Planova.Localization/
└── Resources/                                        # NEW entries
    ├── ActivityResources.en.resx
    └── ActivityResources.ar.resx

tests/
├── Planova.Activity.Tests/                           # NEW
│   ├── Domain/
│   │   ├── ActivityTests.cs
│   │   ├── ActivityRelationshipTests.cs
│   │   ├── CalendarTests.cs
│   │   └── CalendarDayTests.cs
│   ├── Application/
│   │   ├── ActivityServiceTests.cs
│   │   ├── ActivityRelationshipServiceTests.cs
│   │   ├── CircularReferenceDetectorTests.cs
│   │   ├── CalendarDateCalculatorTests.cs
│   │   ├── CalendarServiceTests.cs
│   │   ├── ActivityBankServiceTests.cs
│   │   ├── WbsGenerationServiceTests.cs
│   │   └── ActivityReportServiceTests.cs
│   └── Planova.Activity.Tests.csproj
└── Planova.UI.Tests/
    └── ViewModels/Activity/                          # NEW
        ├── ActivityListViewModelTests.cs
        ├── GanttChartViewModelTests.cs
        ├── RelationshipEditorViewModelTests.cs
        └── WbsGenerationWizardViewModelTests.cs
```

**Structure Decision**: New `Planova.Activity` project following Clean Architecture with its own Domain (entities, enums, interfaces) and Application (services, DTOs) layers. UI components live in existing `Planova.UI` under `Views/Activity/` and `ViewModels/Activity/`. Custom Gantt rendering via `Controls/ActivityGanttCanvas.cs` (DrawingVisual-based Canvas). EF Core configurations added to existing `Planova.Persistence`. Localization resources added to existing `Planova.Localization`. This follows the same pattern as Phase 3 (BOQ Studio) and Phase 4 (WBS Studio). Tests in a new `Planova.Activity.Tests` project plus additions to `Planova.UI.Tests`.

## Complexity Tracking

All gates pass. No complexity justification needed.
