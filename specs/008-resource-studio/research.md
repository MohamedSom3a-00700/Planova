# Research: Resource Studio

## Design Decisions

### 1. Resource Entity Model — Type-Specific Fields

**Decision**: Single `Resource` table with nullable type-specific columns + a discriminator column (`ResourceType`)

**Rationale**:
- EF Core's TPH (Table Per Hierarchy) inheritance maps naturally to the four resource types
- All types share the same core fields (name, code, scope, status)
- Type-specific fields (e.g., `Trade`, `SkillLevel` for Labour; `Capacity`, `OperatingCost` for Equipment) are nullable columns in the same table
- Avoids join complexity of TPT (Table Per Type) and duplication of TPC (Table Per Concrete Type)
- Matches existing patterns in the codebase: single table with discriminator is the simplest approach for <1K resources
- EF Core inheritance with `.HasDiscriminator()` in entity configuration

**Alternatives considered**:
- *JSON column for type-specific attributes*: Rejected — loses type safety, query ability, and EF Core navigation support
- *Separate tables per type (TPT)*: Rejected — excessive joins for listing all resources, no clear benefit at this scale
- *Owned entity per type*: Rejected — EF Core owned types don't support polymorphic queries needed for the resource library listing

### 2. Effective-Dated Rate Resolution

**Decision**: Rate lookup via SQL `MAX(effective_date)` subquery on the application side, cached per resource

**Rationale**:
- `ResourceRate` table with `ResourceId`, `EffectiveDate`, `Rate`, `Currency`, `UnitOfMeasure`
- For any given date, query: `SELECT TOP 1 * FROM ResourceRates WHERE ResourceId = @id AND EffectiveDate <= @date ORDER BY EffectiveDate DESC`
- Fallback to `Resource.DefaultRate` (non-nullable column on Resource) if no matching rate found
- Rate history view: simple ordered query by `EffectiveDate`
- Prevents duplicate effective dates via unique constraint on `(ResourceId, EffectiveDate)`
- Matches the spec requirement (FR-007): "selecting the rate with the latest effective date on or before that date"

**Alternatives considered**:
- *In-memory rate resolution*: Rejected — loading all rates for all resources wastes memory; SQL is more efficient for targeted lookups
- *Daterange/period columns*: Rejected — complex to maintain; effective-date-only is simpler and sufficient
- *Stored procedure*: Rejected — EF Core LINQ can express this; stored procs reduce testability

### 3. Crew Blended Rate Calculation

**Decision**: Computed in-memory on demand (not stored); refreshed when crew composition changes or rates change

**Rationale**:
- Blended rate = `SUM(CrewResource.Quantity * ResolvedRate)` for all crew members
- Storing the blended rate creates staleness problems when resource rates change
- Crew templates typically have <10 resources; in-memory computation is sub-millisecond
- UI calls `ComputeBlendedRate(crewId)` on every display, ensuring accuracy
- Existing rate-at-time-of-application for assignments (per clarifications: "Existing assignments retain the rate at time of application; the crew template's blended rate is recalculated for future applications")

**Alternatives considered**:
- *Pre-computed and stored column*: Rejected — staleness risk and no performance benefit at this scale
- *Database computed column*: Rejected — can't easily use effective-date-based rate resolution in a computed column

### 4. Resource Histogram Computation

**Decision**: Pre-aggregated `ResourceUsage` table populated on assignment save; queried for histogram display

**Rationale**:
- For each assignment with start/end dates, generate daily usage records in a background operation
- `ResourceUsage` table: `Id`, `AssignmentId`, `Date`, `PlannedQuantity`, `ResourceId`
- Histogram query: `GROUP BY Date` with `SUM(PlannedQuantity)` for the selected resource(s)
- Pre-aggregation ensures fast rendering (<5s for 500+ assignments per SC-006)
- Peak markers: compare daily total against `Resource.MaxQuantity` (available quantity)
- Overallocation highlighting: flag days where assigned > available

**Alternatives considered**:
- *On-the-fly date expansion*: Rejected — too slow for 500+ assignments covering months of dates; would require expanding each assignment into daily rows on every render
- *Materialized view*: Rejected — EF Core Migrations don't manage views well; manual maintenance burden

### 5. AI Resource Estimation Integration

**Decision**: Semantic Kernel plugin with a `ResourceEstimationPlugin` that takes activity context and returns structured suggestions

**Rationale**:
- Follows constitution mandate for Semantic Kernel as the AI framework (Principle VI)
- Plugin receives: activity name, description, WBS category, planned dates
- Returns: list of suggested `(ResourceType, ResourceCode, Quantity, Confidence)` tuples
- Abstractions: `IResourceAiEstimationService` interface with `SemanticKernelAiEstimationService` implementation
- Graceful fallback: if SK provider is unavailable, return empty results with descriptive error message
- Configuration-driven: provider choice (Ollama, OpenAI, etc.) from app settings

**Alternatives considered**:
- *Direct OpenAI API call*: Rejected — violates AI Provider Agnostic principle
- *Rule-based estimation*: Rejected — too rigid for diverse construction activities; AI provides better generalization

### 6. Resource Code Auto-Generation

**Decision**: Sequential counter per type stored in a `ResourceCodeCounter` table (or in-memory cache with DB seed)

**Rationale**:
- Pattern: `{Prefix}-{Number:000}` where Prefix depends on type:
  - Labour: `R-LBR`
  - Equipment: `R-EQP`
  - Material: `R-MAT`
  - Subcontractor: `R-SUB`
- Counter stored as a row in the DB per type (could be part of a `Sequence` table)
- On new resource creation: increment counter, format code
- Thread-safe via EF Core concurrency token or serialized access

**Alternatives considered**:
- *MAX(code) + 1 query*: Rejected — race conditions under concurrent access; gaps if resources are deleted
- *GUID-based code*: Rejected — codes must be human-readable per spec (FR-004)

### 7. Global vs Project Resource Scoping

**Decision**: `Resource.Scope` enum with `Global` and `Project` values; `ProjectId` nullable (null for Global)

**Rationale**:
- Global resources visible to all projects; project-scoped resources filtered by `ProjectId`
- Library listing query: resources WHERE `Scope = Global` OR `ProjectId = @currentProjectId`
- Simple, efficient, matches FR-003

**Alternatives considered**:
- *Many-to-many Project-Resource link table*: Rejected — more complex; the spec only requires single-project or global scope, not multi-project sharing

### 8. Import from Excel

**Decision**: Reuse existing Phase 2 Excel infrastructure (ClosedXML) with `ResourceImportReader`

**Rationale**:
- Existing `Planova.Excel` module has readers, writers, and import/export services
- `ResourceImportReader` parses spreadsheet rows into `ResourceImportRow` DTOs
- Duplicate handling per clarifications: detect duplicates (by code/name), present user with skip/overwrite/rename options
- Leverages existing `ImportService` pattern from BOQ/Activity

**Alternatives considered**:
- *Custom CSV parser*: Rejected — Excel is the specified format; existing ClosedXML infrastructure handles it well

### 9. Report Generation

**Decision**: QuestPDF for PDF reports; ClosedXML for spreadsheet export; reuse existing report infrastructure

**Rationale**:
- QuestPDF is the mandated reporting library (constitution Technology Standards)
- Two report types per FR-022: Resource Usage Summary (grouped by activity) and Resource Cost Report (grouped by type, crew, activity)
- Reuse existing `IReportService` pattern from the Application layer

**Alternatives considered**:
- *Direct Printer export*: Rejected — spec requires PDF/spreadsheet export; QuestPDF handles print preview via PDF viewer

### 10. Histogram Visualization

**Decision**: LiveCharts2 `CartesianChart` with `ColumnSeries` for daily bars; `Axis` for timeline

**Rationale**:
- LiveCharts2 is the mandated visualization library (constitution Technology Standards)
- Column series naturally represent daily quantities as bars
- Peak markers via `Series` styling; overallocation highlighting via conditional color mapping
- Filtering by resource type/time range via ViewModel filtering on the data source

**Alternatives considered**:
- *Custom DrawingVisual Canvas*: Rejected — LiveCharts2 provides the charting functionality; custom canvas only used in Activity Studio for Gantt due to specialized rendering needs
