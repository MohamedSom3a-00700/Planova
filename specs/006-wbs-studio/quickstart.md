# Quickstart: WBS Studio

## Prerequisites

- .NET 8 SDK
- Planova development environment set up (follow root README)
- Phase 3 (BOQ Studio) dependencies available for BOQ-to-WBS mapping
- Ollama running locally with Llama 3.2 (optional — AI generation falls back gracefully)

## Getting Started

### 1. Create Planova.Wbs project

```shell
dotnet new classlib -n Planova.Wbs -o Planova.Wbs
```

Reference existing projects:

```shell
dotnet add Planova.Wbs/Planova.Wbs.csproj reference Planova.Domain/Planova.Domain.csproj
dotnet add Planova.Wbs/Planova.Wbs.csproj reference Planova.Boq/Planova.Boq.csproj
```

### 2. Add domain entities

Create entities under `Planova.Wbs/Domain/Entities/` following the [data-model.md](./data-model.md):

- `Wbs.cs` — WBS aggregate root
- `WbsItem.cs` — Tree node with ParentId self-reference
- `WbsTemplate.cs` — Reusable template blueprint
- `WbsTemplateItem.cs` — Template tree node

### 3. Add value objects and enums

Create under `Planova.Wbs/Domain/Enums/`:

- `WbsStatus.cs` — Draft, Final, Revised, Approved (with transition validation)
- `WbsLevelType.cs` — Summary, ControlAccount, WorkPackage, PlanningPackage
- `WbsSource.cs` — Manual, FromBOQ, FromTemplate, AIGenerated

### 4. Add repository interfaces

Create under `Planova.Wbs/Domain/Interfaces/`:

- `IWbsRepository.cs`
- `IWbsItemRepository.cs`
- `IWbsTemplateRepository.cs`
- `IWbsValidationService.cs`
- `IWbsBoqMappingService.cs`
- `IWbsAiGenerationService.cs`
- `IWbsReportService.cs`

### 5. Add application services

Create under `Planova.Wbs/Application/Services/`:

- `WbsService.cs` — CRUD, tree queries, weight distribution
- `WbsItemService.cs` — CRUD for items, reorder, bulk update
- `WbsValidationService.cs` — Structural and data validation
- `WbsBoqMappingService.cs` — BOQ-to-WBS mapping (3 strategies)
- `WbsTemplateService.cs` — Template CRUD, apply, import/export
- `WbsAiGenerationService.cs` — AI-powered WBS suggestion
- `WbsReportService.cs` — Summary and dictionary report queries

### 6. Add persistence configurations

Create under `Planova.Persistence/EntityConfigurations/`:

- `WbsConfiguration.cs`
- `WbsItemConfiguration.cs`
- `WbsTemplateConfiguration.cs`
- `WbsTemplateItemConfiguration.cs`

Then create and apply a migration:

```shell
dotnet ef migrations add AddWbsEntities -p Planova.Persistence
dotnet ef database update -p Planova.Persistence
```

Seed standard templates in the migration.

### 7. Add UI views and viewmodels

Create under `Planova.UI/Views/Wbs/` and `Planova.UI/ViewModels/Wbs/`:

- WBS List — browse, search, create with source dialog
- WBS Tree Viewer — expand/collapse, color-coded, weight bars
- WBS Editor — inline editing, add/delete/reorder
- BOQ Mapping Wizard — 3-step mapping workflow
- Template Manager — CRUD, apply, import/export
- AI Generation Panel — scope input, generate, accept/regenerate
- Reports — summary + dictionary, Excel/PDF export

### 8. Register navigation

Add WBS Studio entry to the navigation rail in `ShellViewModel` and wire to `WbsTreeView`.

### 9. Add localization resources

Add strings to `Planova.Localization/Resources/`:

- `WbsResources.en.resx` — English strings
- `WbsResources.ar.resx` — Arabic strings

## Key Patterns

- **Tree structure**: Same as Phase 3 — `ParentId` self-reference, recursive CTE queries, in-memory tree building for UI
- **Virtualized tree**: Reuse `VirtualizingTreeView` pattern from Phase 3
- **BOQ mapping**: Read-only access to `Planova.Boq` repositories
- **AI**: Use `Semantic Kernel` with `IAIProvider` abstraction; structure output as JSON
- **Weight distribution**: Percentage-based, siblings sum to 100%
- **Code generation**: Two auto-generated codes — numeric (position-based) and alpha (name-based)

## Testing

```shell
# Run WBS domain tests
dotnet test tests/Planova.Wbs.Tests

# Run WBS UI viewmodel tests
dotnet test tests/Planova.UI.Tests --filter "FullyQualifiedName~Wbs"

# Run all tests
dotnet test
```

## References

- [Specification](./spec.md)
- [Data Model](./data-model.md)
- [Implementation Plan](./plan.md)
- [Research Notes](./research.md)
- [Planova Constitution](../../.specify/memory/constitution.md)
- Phase 3 BOQ Studio patterns: `specs/005-boq-studio/`
