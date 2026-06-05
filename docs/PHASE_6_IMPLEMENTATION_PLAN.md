# Planova Phase 6 Implementation Plan

**Phase**: 6 — Resource Studio

**Date**: 2026-06-05

**Source of Truth**: [docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/02-MASTER_ROADMAP.md](./02-MASTER_ROADMAP.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/05-TECHNOLOGY_STACK.md](./05-TECHNOLOGY_STACK.md),
[docs/06-MODULE_CATALOG.md](./06-MODULE_CATALOG.md),
[docs/07-DATABASE_STRATEGY.md](./07-DATABASE_STRATEGY.md),
[docs/08-INTEGRATION_STRATEGY.md](./08-INTEGRATION_STRATEGY.md),
[docs/09-AI_STRATEGY.md](./09-AI_STRATEGY.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[docs/PLANOVA_CONSTITUTION_DRAFT.md](./PLANOVA_CONSTITUTION_DRAFT.md),
[docs/PHASE_5_IMPLEMENTATION_PLAN.md](./PHASE_5_IMPLEMENTATION_PLAN.md),
[specs/007-activity-studio/data-model.md](../specs/007-activity-studio/data-model.md)

## Summary

Phase 6 delivers the **Resource Studio** — the fifth specialized studio in the Planova project controls workflow and the natural next step after Activity Studio (Phase 5). This phase transforms scheduled activities into resource-loaded project plans by adding labour, equipment, material, and subcontractor resource management, crew templates, a resource histogram, and AI-powered resource estimation.

The ultimate workflow progression:

```
BOQ (Phase 3) → WBS (Phase 4) → Activities (Phase 5) → Resources (Phase 6) → Cost Loading (Phase 7)
```

Resource management is a core project controls function. Planning engineers need to assign labour crews, equipment, materials, and subcontractors to each activity to compute resource costs, identify resource conflicts, generate histograms, and feed cost data downstream to Phase 7 (Cost Studio). The **Resource Library** serves as a reusable master database of standard resources (trades, equipment types, materials, subcontractors) that can be shared across projects or defined per-project.

The **Crew Template** system allows users to compose multi-resource crews (e.g., "Concrete Crew: 3 Carpenters + 1 Foreman + 2 Laborers") with auto-calculated blended rates and apply them to activities in one click.

The **AI Resource Estimation** service uses Semantic Kernel with IAIProvider abstraction to suggest resource requirements from activity descriptions — helping users quickly populate resource assignments without starting from scratch.

This phase does not cover cost loading, cash flow, earned value, or resource leveling. Those are separate downstream phases (7, 18) that will consume the structured resource assignment output.

## Phase 6 Objectives

1. Introduce the Resource domain model (Resource with TPH for Labour/Equipment/Material/Subcontractor, ResourceRate, Crew, CrewResource, ResourceAssignment, ResourceUsage).
2. Build a resource library with type-aware editors (Labour rates, Equipment costs, Material prices, Subcontractor contracts).
3. Implement resource rate management with effective dating and history tracking.
4. Build a Crew Template system — compose multi-resource crews, calculate blended rates, apply to activities.
5. Implement resource loading onto Phase 5 activities — assign resources/crews with quantities and rates.
6. Build a Resource Histogram chart using LiveCharts2 showing usage over time with peak and overallocation detection.
7. Implement AI resource estimation using Semantic Kernel + IAIProvider — suggest resource requirements from activity descriptions.
8. Deliver resource usage and cost reports with Excel and PDF export.
9. Preserve Clean Architecture, MVVM, localization (English + Arabic), and theme consistency.
10. Ensure the resource data model is structured for consumption by Phase 7 (Cost Studio).

## Phase 6 Scope

### In Scope

- Resource domain entity (TPH: Labour, Equipment, Material, Subcontractor) with global + project scope
- ResourceRate entity with effective date ranges and multiple rate types
- Crew and CrewResource entities for reusable crew templates
- ResourceAssignment entity linking resources to Phase 5 activities
- ResourceUsage entity for daily tracking and histogram data
- Resource library CRUD with type-specific field editors
- Resource rate history manager (add/edit future rates, bulk updates)
- Crew template composer (add resources with quantities, mark lead, calculate blended rate)
- Resource loading view (select activity → assign resources with qty/rate/dates)
- Apply crew to single or multiple activities in one operation
- Resource histogram (LiveCharts2 bar chart, filter by resource/type, peak detection, overallocation warnings)
- AI Resource Estimation (Semantic Kernel + IAIProvider, structured JSON output, preview/accept/adjust)
- Resource usage report (by activity, by resource, by type)
- Resource cost report
- Export reports to Excel (reuse Phase 2 WorkbookWriter) and PDF (QuestPDF)
- Import resource library from Excel (reuse Phase 2 IWorkbookReader)
- Navigation rail integration — Resource Studio opens in multi-tab workspace
- Database persistence — new tables: Resources, ResourceRates, Crews, CrewResources, ResourceAssignments, ResourceUsage
- Localization (English and Arabic) for all Resource screens
- Unit tests for domain, services, validation, AI estimation
- Seed data for standard global resources (common trades, equipment types, material categories)

### Out of Scope

- Cost loading or cash flow (Phase 7)
- Earned value or performance metrics (Phase 18)
- Primavera P6 XER resource import/export (Phase 9)
- Resource leveling or smoothing (deferred)
- Resource vs budget variance analysis
- Multi-user collaboration on resources (Phase 19)
- Automated resource optimization
- Resource productivity tracking against baseline
- Purchase order or procurement workflows

## Non-Negotiable Constraints

- Clean Architecture must remain intact.
- MVVM must remain the UI pattern.
- Localization must support English and Arabic.
- RTL behavior must remain correct.
- Theme support must remain consistent with Phases 0/1/2/3/4/5.
- Phase 5 Activity module must be consumed (not duplicated) — ResourceAssignment.ActivityId FK.
- Phase 2 Excel/PDF infrastructure must be reused for reports and import.
- AI provider must be abstracted via IAIProvider — no direct vendor lock-in.
- Resource data model must support both Global (shared) and Project-specific resources.
- Resource type discriminator must use TPH (single Resources table with ResourceType column).
- The Resource data model must accommodate downstream Phase 7 Cost integration.
- All async operations must accept CancellationToken.

## Phase 6 Product Shape

Phase 6 should feel like the natural resource companion to Activity Studio.

The user should be able to:

- browse the resource library filtered by type (Labour, Equipment, Material, Subcontractor)
- create new resources with type-specific fields (trade, skill level, hourly rate for labour; capacity, operating cost for equipment; unit price, wastage for materials; company, contact, contract value for subcontractors)
- manage resource rates over time with effective date ranges
- view rate history and future rate changes
- compose crew templates by adding multiple resources with quantities
- see the blended hourly/daily rate for a crew calculated automatically
- assign resources to activities in the resource loading view
- apply a crew template to one or more activities, generating resource assignments for each crew member
- view a resource histogram showing daily usage across the project timeline
- see peak usage markers and overallocation warnings on the histogram
- use AI to estimate resource requirements from an activity description
- generate resource usage and cost reports
- export reports to Excel and PDF
- import a resource library from an Excel file

## Target Solution Impact

Phase 6 extends the foundation established in Phases 0, 1, 2, 3, 4, and 5.

Expected additions:

- new Domain entities and enums in a new `Planova.Resource` project
- new Application services for Resource use cases
- new Infrastructure/Persistence mappings, repositories, and migrations
- new UI modules for Resource Studio (library, rate manager, crew composer, loading view, histogram, AI panel, reports)
- expanded localization resources for Resource terminology
- seeded global resource data (common trades, standard equipment, material categories)
- AI provider integration for resource estimation

## Primary Domain Model

### Resource (TPH — single table with ResourceType discriminator)

Represents any resource that can be assigned to activities. Uses Table-Per-Hierarchy with type-specific nullable columns.

Properties:

- Id (Guid)
- ProjectId (int?, FK → Projects — null = Global resource)
- Code (string — auto-generated per type, e.g. "R-LBR-001", "R-EQP-001", "R-MAT-001", "R-SUB-001")
- Name (string)
- Description (string?)
- ResourceType (ResourceType: Labour, Equipment, Material, Subcontractor)
- UnitOfMeasure (string — HR, DAY, M3, TON, EA, M2, LS, etc.)
- Category (string?)
- DefaultRate (decimal?)
- IsActive (bool)
- IsStandard (bool — system-seeded vs user-created)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

**Labour-specific properties** (nullable for non-Labour):

- Trade (string? — Carpenter, Steel Fixer, Formwork Carpenter, General Laborer, etc.)
- SkillLevel (SkillLevel? — Skilled, SemiSkilled, Unskilled)
- DefaultHourlyRate (decimal?)
- OvertimeRate (decimal?)
- DefaultHoursPerDay (int?, default 8)

**Equipment-specific properties** (nullable for non-Equipment):

- EquipmentType (string? — Crane, Excavator, Concrete Pump, Dump Truck, etc.)
- Capacity (decimal?)
- CapacityUnit (string? — ton, m³, kW)
- DefaultHourlyRate (decimal?)
- OperatingCost (decimal?)
- OwnershipCost (decimal?)
- FuelType (string?)

**Material-specific properties** (nullable for non-Material):

- MaterialType (string? — Concrete, Rebar, Formwork, Aggregate, etc.)
- DefaultUnitPrice (decimal?)
- WastageFactor (decimal? — 0-100, percentage)
- LeadTimeDays (int?)

**Subcontractor-specific properties** (nullable for non-Subcontractor):

- CompanyName (string?)
- ContactPerson (string?)
- Phone (string?)
- Email (string?)
- Trade (string?)
- ContractValue (decimal?)
- MobilizationDate (DateTime?)

### ResourceRate

Represents a resource rate valid from an effective date.

Properties:

- Id (Guid)
- ResourceId (Guid, FK → Resource)
- EffectiveDate (DateTime — date from which this rate applies)
- Rate (decimal)
- RateType (RateType: Hourly, Daily, Weekly, Monthly, Unit, LumpSum)
- Currency (string — default "USD")
- CreatedAt (DateTime)

Validation:

- Rate must be > 0
- EffectiveDate must be unique per ResourceId (no two rates with same date)
- RateType must be valid

### Crew

Represents a reusable template of grouped resources that work together.

Properties:

- Id (Guid)
- ProjectId (int?, FK → Projects — null = Global crew)
- Name (string — e.g. "Concrete Crew", "Steel Fixing Crew")
- Description (string?)
- Category (string? — Concrete, Steel, Finishing, MEP, etc.)
- DefaultProductivity (decimal?)
- ProductivityUnit (string? — m³/day, m²/day, tons/day)
- BlendedRate (decimal — computed from CrewResources)
- CreatedAt
- UpdatedAt

Navigation:

- Resources (ICollection<CrewResource>)

### CrewResource

Links a resource (with quantity) to a crew.

Properties:

- Id (Guid)
- CrewId (Guid, FK → Crew)
- ResourceId (Guid, FK → Resource)
- Quantity (decimal — number of units of this resource in the crew)
- IsLead (bool — marks foreman/lead)

### ResourceAssignment

Links a resource or crew to a Phase 5 activity with quantity, rate, and dates.

Properties:

- Id (Guid)
- ActivityId (Guid, FK → Activity — from Phase 5 Planova.Activity)
- ProjectId (int, FK → Projects)
- ResourceId (Guid?, FK → Resource — null if using CrewId)
- CrewId (Guid?, FK → Crew — null if direct resource assignment)
- Quantity (decimal)
- Unit (string)
- Rate (decimal)
- TotalCost (decimal, computed = Quantity × Rate, or Quantity × Duration × Rate for Hourly)
- StartDate (DateTime?)
- EndDate (DateTime?)
- AssignmentType (AssignmentType: Normal, Overtime)
- Notes (string?)
- CreatedAt
- UpdatedAt

Validation:

- Either ResourceId or CrewId must be set (not both null)
- ActivityId must reference an existing activity from Phase 5
- Quantity must be > 0
- Rate must be ≥ 0
- StartDate must precede EndDate (if both set)
- TotalCost computed on save

### ResourceUsage

Tracks planned and actual resource usage per day for histogram computation.

Properties:

- Id (Guid)
- ResourceAssignmentId (Guid, FK → ResourceAssignment)
- Date (DateTime — date-only)
- PlannedQuantity (decimal?)
- ActualQuantity (decimal?)
- CreatedAt

### Value Objects

- **ResourceType** — Labour, Equipment, Material, Subcontractor
- **SkillLevel** — Skilled, SemiSkilled, Unskilled
- **RateType** — Hourly, Daily, Weekly, Monthly, Unit, LumpSum
- **AssignmentType** — Normal, Overtime

## Screen Plan

### 1. Resource Library Workspace

- Tabbed resource list filtered by type (Labour, Equipment, Material, Subcontractor)
- Search by name, code, category
- Filter by project (Global vs current project)
- Summary cards (total resources, by type, by project scope)
- Create new resource with type selector dialog
- Open, edit, delete actions
- Type indicator badges (color-coded: blue=Labour, green=Equipment, orange=Material, purple=Subcontractor)
- Import from Excel button

### 2. Resource Editor

Type-aware form that changes fields based on ResourceType:

**Labour fields**: Trade, Skill Level (dropdown), Default Hourly Rate, Overtime Rate, Hours Per Day, Unit of Measure

**Equipment fields**: Equipment Type, Capacity, Capacity Unit, Default Hourly Rate, Operating Cost, Ownership Cost, Fuel Type, Unit of Measure

**Material fields**: Material Type, Default Unit Price, Wastage Factor (%), Lead Time (days), Unit of Measure

**Subcontractor fields**: Company Name, Contact Person, Phone, Email, Trade, Contract Value, Mobilization Date

Common fields for all types: Code (auto-generated), Name, Description, Category, Default Rate, Is Active

### 3. Resource Rate Manager

- Rate list for selected resource with effective dates
- Add new rate (effective date, rate, rate type)
- Rate history graph (rate over time)
- Bulk rate update (select resources by type/category, set new rate + effective date)
- Rate change preview before commit
- Copy rates from one resource to another

### 4. Crew Template Manager

- Crew list with cards showing composition summary (e.g., "3 Carpenters + 1 Foreman + 2 Laborers")
- Create new crew (name, description, category, productivity)
- Crew composer:
  - Left panel: available resources (filter by type, search)
  - Right panel: crew composition grid (resource, quantity, is lead, individual rate, total rate)
  - Auto-calculated blended rate at the bottom
  - Drag resources from left to right
- Apply crew to activity dialog (select target activity, confirm assignments)
- Bulk apply crew to multiple activities
- Clone crew
- Create crew from existing resource assignments (reverse-engineer composition)

### 5. Resource Loading View

Split-panel layout:

- **Left panel**: Activity list from Phase 5 (filterable by project, WBS, status)
  - Columns: Code, Name, Duration, Planned Start, Planned Finish
  - Search by name/code
  - Select activity → right panel shows current assignments

- **Right panel**: Resource assignments for selected activity
  - Grid: Resource/Crew, Quantity, Unit, Rate, Total Cost, Start, End, Type
  - Add resource: search/browse resource library
  - Add crew: search/browse crew templates
  - Inline edit quantity, rate, dates
  - Delete assignment with confirmation
  - Total resource cost summary for the activity

### 6. Resource Histogram

- LiveCharts2 bar chart showing daily resource quantity usage
- Filter controls:
  - Resource type (Labour, Equipment, Material, or All)
  - Specific resource or crew selector
  - Time range (Full Project, This Month, Custom Range)
  - Aggregation (Sum, Average, Peak)
- Zoom: day/week/month buttons
- Color-coded bars by resource type
- Peak usage markers (dashed line at max)
- Overallocation warning: when usage exceeds available quantity, bars turn red
- Data table below chart (Date, Resource, Planned Qty, Actual Qty, Variance)
- Export chart to image / export data to Excel

### 7. AI Resource Estimation Panel

- Activity selector (or use currently selected activity from loading view)
- "Estimate Resources" button
- AI progress indicator during generation
- Results display:
  - Suggested labour crew with trades and quantities
  - Suggested equipment with types and quantities
  - Suggested materials with names and quantities
  - Confidence indicator per suggestion
- Accept (creates assignments), Adjust (modify quantities before accepting), Reject buttons
- "Was this helpful?" feedback toggle (Yes/No) for AI improvement
- Manual override: user can edit any AI-suggested value

### 8. Resource Reports

- **Resource Usage Summary Report**: hierarchical by activity → resource, showing quantities and costs
- **Resource Cost Report**: totals by resource type, by crew, by activity
- **Resource Histogram Data Export**: raw daily usage data
- Export to Excel (reuse Phase 2 WorkbookWriter)
- Export to PDF (QuestPDF)
- Print preview

## Implementation Order

### 1. Domain Model and Contracts

- ResourceType, SkillLevel, RateType, AssignmentType enums
- Resource entity (TPH base class with type-specific properties)
- ResourceRate entity
- Crew entity
- CrewResource entity
- ResourceAssignment entity
- ResourceUsage entity
- Repository interfaces (IResourceRepository, IResourceRateRepository, ICrewRepository, ICrewResourceRepository, IResourceAssignmentRepository, IResourceUsageRepository)
- Service interfaces (IResourceService, IResourceRateService, ICrewService, IResourceAssignmentService, IResourceHistogramService, IResourceEstimationService, IResourceReportService)

### 2. Persistence Schema and Mappings

- EF Core entity configurations for all 6 new entities
- TPH mapping for Resource with ResourceType discriminator
- Relationships: ResourceAssignment → Activity (Phase 5), ResourceAssignment → Resource, Crew → CrewResource → Resource
- Indexes on Resource: ProjectId, ResourceType, Code, Category
- Indexes on ResourceRate: ResourceId, EffectiveDate
- Indexes on Crew: ProjectId, Category
- Indexes on CrewResource: CrewId, ResourceId
- Indexes on ResourceAssignment: ActivityId, ResourceId, CrewId, ProjectId
- Indexes on ResourceUsage: ResourceAssignmentId, Date
- Migration for Resource tables
- Seed data for global default resources (common trades, standard equipment, material categories)

### 3. Application Services

- IResourceService — CRUD with type-aware creation, filter by project/type/category, search
- IResourceRateService — CRUD with effective dating, GetRateAtDateAsync, bulk rate update
- ICrewService — CRUD, blended rate computation, apply crew to activities, clone
- IResourceAssignmentService — CRUD, bulk assign, total cost computation, get by activity/resource
- IResourceHistogramService — daily usage computation, peak detection, overallocation check
- IResourceEstimationService — AI-powered estimation with Semantic Kernel (IAIProvider)
- IResourceReportService — usage summary and cost report queries

### 4. Resource Rate Engine

- Rate resolution: GetApplicableRateAsync(resourceId, date) — finds the rate with the latest EffectiveDate ≤ the given date
- Bulk rate update: create new rates for all resources matching a filter (type, category)
- Rate history: return all rates for a resource ordered by EffectiveDate
- Rate projection: calculate rate at future dates for cost forecasting

### 5. Crew Rate Computation

- Blended rate = Σ (CrewResource.Quantity × Resource.DefaultRate or applicable ResourceRate)
- Display hourly, daily, and weekly blended rates
- Recompute on any CrewResource add/update/delete
- Apply crew to activity: for each CrewResource, create a ResourceAssignment with the resource, quantity, and computed rate

### 6. Resource Histogram Engine

- Compute daily usage: for each ResourceAssignment with StartDate/EndDate, spread Quantity evenly across working days (using Phase 5 CalendarDateCalculator)
- Aggregate by resource, type, or crew
- Find peak usage date
- Compare against available quantity (for overallocation)
- Cache results with invalidation on assignment changes

### 7. AI Resource Estimation

- Semantic Kernel integration with IAIProvider abstraction
- Prompt template: activity name, description, WBS category, duration → JSON resource suggestion
- Structured output parsing into ResourceEstimateDto
- Fallback to manual if AI unavailable or times out
- Feedback collection for future model improvement

### 8. UI — Resource Navigation and Registration

- Register Resource Studio nav target in ShellViewModel
- Create ResourceStudioView as the main workspace container
- Create ResourceStudioViewModel with tab initialization
- Wire nav rail item with Toolbox icon (or resource-appropriate icon)
- Ensure Resource Studio opens in multi-tab workspace

### 9. UI — Resource Library and Editor

- Resource list view with type tabs, search, filter
- Resource editor view with dynamic type-aware fields
- Create/Edit/Delete workflows
- Import from Excel button (reuse Phase 2)

### 10. UI — Resource Rate Manager

- Rate table per resource
- Add rate form (effective date picker, rate input, rate type dropdown)
- Rate history chart
- Bulk rate update dialog

### 11. UI — Crew Template Manager

- Crew list with composition summary cards
- Crew composer with resource picker and composition grid
- Blended rate display
- Apply to activity dialog
- Bulk apply dialog

### 12. UI — Resource Loading View

- Split panel: activity list + assignment grid
- Resource picker dialog (search/browse library)
- Crew picker dialog
- Inline editing of quantity, rate, dates
- Total cost summary per activity

### 13. UI — Resource Histogram

- LiveCharts2 bar chart control
- Filter panel (type, resource, crew, time range, aggregation)
- Zoom controls
- Peak and overallocation overlays
- Data table below chart

### 14. UI — AI Estimation Panel

- Activity selector
- Estimate button with progress
- Results display with accept/adjust/reject
- Feedback toggle

### 15. UI — Resource Reports

- Usage summary report view
- Cost report view
- Excel export (reuse Phase 2 WorkbookWriter)
- PDF export (QuestPDF)
- Print preview

### 16. Localization and RTL Coverage

- English and Arabic resources for all Resource screens
- Terms: resource, labour, equipment, material, subcontractor, crew, rate, assignment, histogram, estimate
- RTL verification for all Resource views

### 17. Validation

- Resource CRUD smoke tests
- Resource rate effective dating tests
- Crew composition and blended rate tests
- Resource assignment CRUD tests
- Resource histogram computation tests
- AI estimation service tests (with mock provider)
- Report generation tests
- Localization correctness tests
- Persistence round-trip tests

## Technical Decisions

### TPH Inheritance for Resources

Use a single `Resources` table with `ResourceType` discriminator column. Type-specific properties are nullable columns. This is the simplest approach for Phase 6 — avoids complex joins, allows a single repository, and makes it easy to add new resource types in the future. If type-specific constraints or performance requirements emerge, migration to TPT is possible later.

Decision: TPH with string discriminator ("Labour", "Equipment", "Material", "Subcontractor").

### Resource Code Auto-Generation

Sequential codes per resource type with prefix:
- Labour: "R-LBR-001", "R-LBR-002"
- Equipment: "R-EQP-001", "R-EQP-002"
- Material: "R-MAT-001", "R-MAT-002"
- Subcontractor: "R-SUB-001", "R-SUB-002"

Generated via `IResourceRepository.GetNextCodeAsync(ResourceType)`.

### Resource Rate Resolution

Rates are resolved by finding the `ResourceRate` with the latest `EffectiveDate` ≤ the reference date (default: today). If no rate exists, fall back to `Resource.DefaultRate`.

### Crew Blended Rate Calculation

```
BlendedHourlyRate = Σ(CrewResource.Quantity × EffectiveHourlyRateOfResource)
```

The blended rate is a computed property — recalculated in the Application layer whenever CrewResources are added/removed/updated.

### Phase 5 Activity Consumption

Resource assignments reference activities via `ActivityId` (Guid FK). The Activity entity lives in `Planova.Activity.Domain.Entities`. `Planova.Resource` references `Planova.Activity` for this FK.

### Resource Histogram Computation

Daily usage is computed by:
1. For each ResourceAssignment with StartDate and EndDate
2. Determine working days in range (using Phase 5 CalendarDateCalculator with the activity's assigned calendar, or project default)
3. Distribute `Quantity` evenly across working days
4. Aggregate by date + resource for the histogram series

Computation happens in the Application layer (IResourceHistogramService), not in SQL. Cached and invalidated on assignment changes.

### AI Estimation Architecture

Follows the same pattern as Phase 4's AI WBS generation:
- `IResourceEstimationService` depends on `IAIProvider` (from Planova.AI or shared contract)
- Prompt engineered to return structured JSON
- User previews before committing
- Default provider: Ollama + Llama 3.2

Prompt structure:
```
Given a construction activity with name "{Name}", description "{Description}",
WBS category "{Category}", and duration {Duration} days,
suggest appropriate resource requirements as a JSON object with
labour (array of {trade, quantity, unit}), equipment (array of {type, quantity, unit}),
and materials (array of {name, quantity, unit}).
```

### Resource Library Seeding

Seed global resources on first access (lazy seeding pattern from Phase 5):

**Labour seeds**: Carpenter, Steel Fixer, Formwork Carpenter, General Laborer, Equipment Operator, Mason, Plumber, Electrician, Welder, Painter, Plasterer, Tiler, Scaffolder, Rigger, Crane Operator, Concrete Finisher, Surveyor, Safety Officer

**Equipment seeds**: Tower Crane, Mobile Crane, Excavator, Bulldozer, Concrete Pump, Dump Truck, Forklift, Roller Compactor, Grader, Generator, Welding Machine, Air Compressor, Concrete Mixer, Vibrator, Scaffolding System, Formwork System

**Material seeds**: Ready Mix Concrete (various grades), Rebar (various diameters), Formwork Plywood, Cement, Aggregate, Sand, Blocks, Bricks, Steel Beams, Steel Columns, Pipes (various), Cables, Conduit, Tiles, Paint, Waterproofing Membrane

**Subcontractor seeds**: (none — always project-specific)

## Detailed Work Breakdown

### Workstream A: Domain and Schema

1. Define ResourceType, SkillLevel, RateType, AssignmentType enums
2. Define Resource entity with TPH discriminator and type-specific properties
3. Define ResourceRate entity
4. Define Crew entity
5. Define CrewResource entity
6. Define ResourceAssignment entity
7. Define ResourceUsage entity
8. Define value objects and validation rules
9. Add entity configurations and relationships
10. Create migration and apply
11. Add indexes for common query paths
12. Seed global default resources

### Workstream B: Repository and Service Interfaces

1. IResourceRepository — CRUD with type/project filtering
2. IResourceRateRepository — CRUD with effective date queries
3. ICrewRepository — CRUD, composition loading
4. ICrewResourceRepository — CRUD within crew context
5. IResourceAssignmentRepository — CRUD, by activity, by resource
6. IResourceUsageRepository — daily tracking, range queries
7. IResourceService, IResourceRateService, ICrewService, IResourceAssignmentService, IResourceHistogramService, IResourceEstimationService, IResourceReportService interfaces

### Workstream C: Resource Management Services

1. Implement IResourceService (CRUD, type-aware creation, filter/search)
2. Implement resource code auto-generation
3. Implement IResourceRateService (CRUD, effective date resolution, bulk update)
4. Implement rate history and projection logic

### Workstream D: Crew Engine

1. Implement ICrewService (CRUD, blend rate computation)
2. Implement CrewResource management (add/remove resources, update quantities)
3. Implement ApplyCrewToActivityAsync (creates ResourceAssignments)
4. Implement BulkApplyCrewToActivitiesAsync
5. Implement reverse-engineer crew from existing assignments

### Workstream E: Resource Assignment Engine

1. Implement IResourceAssignmentService (CRUD, total cost computation)
2. Implement get assignments by activity/resource/project
3. Implement assignment validation (dates, quantity, rate)
4. Implement bulk assignment operations

### Workstream F: Resource Histogram Engine

1. Implement IResourceHistogramService (daily usage computation)
2. Implement working day distribution (integrate with Phase 5 CalendarDateCalculator)
3. Implement peak detection and overallocation checking
4. Implement histogram data DTO assembly

### Workstream G: AI Resource Estimation

1. Implement IResourceEstimationService with Semantic Kernel
2. Design resource estimation prompt template
3. Implement structured JSON output parsing
4. Implement preview/accept/adjust workflow
5. Handle AI unavailability gracefully (fallback to manual)

### Workstream H: UI — Resource Navigation and Registration

1. Register Resource Studio nav target in ShellViewModel
2. Create ResourceStudioView as the main workspace container
3. Create ResourceStudioViewModel with tab initialization
4. Wire nav rail item with appropriate icon

### Workstream I: UI — Resource Library and Editor

1. Create resource list view with type tabs, search, filter
2. Create type-aware resource editor view
3. Create create/edit/delete workflows
4. Create import from Excel dialog

### Workstream J: UI — Resource Rate Manager

1. Create rate list view per resource
2. Create add rate form with effective date picker
3. Create rate history chart view
4. Create bulk rate update dialog

### Workstream K: UI — Crew Template Manager

1. Create crew list with composition summary cards
2. Create crew composer view (resource picker + composition grid)
3. Create blended rate display
4. Create apply to activity/bulk dialogs

### Workstream L: UI — Resource Loading View

1. Create split-panel layout (activity list + assignments)
2. Create resource picker dialog
3. Create crew picker dialog
4. Create assignment grid with inline editing
5. Create total cost summary display

### Workstream M: UI — Resource Histogram

1. Create LiveCharts2 bar chart control
2. Create filter panel (type, resource, crew, time range)
3. Create zoom controls
4. Create peak/overallocation overlays
5. Create data table below chart

### Workstream N: UI — AI Estimation Panel

1. Create activity selector
2. Create estimate button with progress indicator
3. Create results display with accept/adjust/reject
4. Create feedback toggle

### Workstream O: UI — Resource Reports

1. Create usage summary report view
2. Create cost report view
3. Implement Excel export (reuse Phase 2 WorkbookWriter)
4. Implement PDF export (QuestPDF)
5. Implement print preview

### Workstream P: Localization and Polish

1. Add English resource strings for all Resource screens
2. Add Arabic resource strings for all Resource screens
3. Verify Arabic layout and RTL behavior
4. Ensure theme consistency with existing shell
5. Remove any hardcoded user-facing strings
6. Verify navigation rail Resource icon and highlighting

### Workstream Q: Testing

1. Unit test domain entities and value objects
2. Unit test Resource CRUD services
3. Unit test ResourceRate effective dating
4. Unit test Crew composition and blended rate calculation
5. Unit test ResourceAssignment total cost computation
6. Unit test ResourceHistogram daily usage computation
7. Unit test AI estimation service (with mock provider)
8. Integration test persistence round-trip
9. Integration test import from Excel
10. UI smoke test all Resource workspaces

## Risks and Mitigations

### Risk: TPH model becomes unwieldy with many type-specific columns

Mitigation:
- Keep type-specific properties focused on the essential fields per type
- Consider JSON columns for rarely-used extended properties
- Migration to TPT is possible if needed in a later phase

### Risk: Resource rate effective dating logic is complex

Mitigation:
- Comprehensive unit tests for date-based rate resolution
- Visual preview of rate history in UI
- Clear validation for overlapping effective dates

### Risk: Crew blended rate calculation is slow with large crews

Mitigation:
- Cache resource rates at computation time
- Recompute only when CrewResources or resource rates change
- Display loading indicator during computation

### Risk: Resource histogram performance with large datasets

Mitigation:
- Compute daily usage in Application layer (not SQL)
- Cache histogram data, invalidate on assignment changes
- Limit default date range to current year
- Virtualized chart rendering via LiveCharts2

### Risk: AI resource estimation quality is inconsistent

Mitigation:
- Provider abstraction allows swapping models
- AI output is always suggested — never committed without user review
- User can adjust quantities before accepting
- Feedback collection for prompt refinement
- Fallback to manual estimation if AI unavailable

### Risk: Phase 5 Activity module needs modification for resource integration

Mitigation:
- Access Activities through Phase 5 repository interfaces
- Keep resource assignment logic in Planova.Resource, not in Planova.Activity
- Extend Phase 5 contracts with backward-compatible additions if needed

## Acceptance Criteria

Phase 6 is complete when all of the following are true:

- Resources can be created, edited, and persisted with type-specific fields.
- Resources support both Global (shared) and Project-specific scope.
- Resource rates can be managed with effective dates and history tracking.
- Crew templates can be composed with multiple resources and auto-calculated blended rates.
- Crews can be applied to activities, generating resource assignments for each crew member.
- Resources can be loaded onto Phase 5 activities with quantity, rate, and dates.
- Resource assignments compute total cost correctly.
- The resource histogram displays daily usage with peak markers and overallocation warnings.
- AI can suggest resource requirements from an activity description (with manual acceptance).
- Resources can be imported from Excel.
- Resource reports (usage and cost) can be generated and exported to Excel and PDF.
- English and Arabic screens work correctly.
- RTL behavior remains correct.
- The existing shell, theme, and infrastructure from Phases 0–5 remain intact.
- The implementation remains Clean Architecture and MVVM compliant.

## Definition of Done

- Core Resource domain entities are implemented.
- Resource CRUD, rate management, crew templates, resource loading, and histogram services are functional.
- Resource Studio is integrated into the shell with navigation registration.
- Global default resources are seeded.
- AI resource estimation is functional with provider abstraction.
- Resource histogram is functional with LiveCharts2 integration.
- Resource reports are functional with Excel and PDF export.
- Localization is complete for all new Resource scope.
- All tests pass.
- The data model is documented for Phase 7 (Cost Studio) consumption.

## Next Step After Phase 6

When Resource Studio is stable, the next implementation plan should target Phase 7 — Cost Studio:

- Cost loading from resource assignments
- Cash flow forecasting
- Budget management
- Earned value management
- Resource cost integration
