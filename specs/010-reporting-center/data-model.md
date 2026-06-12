# Data Model: Reporting Center

## Entities

### ReportInstance

A generated report with frozen data snapshot, status lifecycle, and AI narrative text.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly, Executive |
| `TemplateId` | `Guid?` | No | FK → ReportTemplate |
| `Title` | `string(200)` | Yes | e.g. "Weekly Report — Week 24, 2026" |
| `Status` | `ReportStatus` | Yes | Draft, Final, Archived. Forward-only lifecycle |
| `PeriodStart` | `DateTime` | Yes | Report period start |
| `PeriodEnd` | `DateTime` | Yes | Report period end |
| `GeneratedAt` | `DateTime` | Yes | |
| `GeneratedBy` | `string(100)?` | No | |
| `DataSnapshotJson` | `string` | Yes | Frozen report data at generation time (JSON) |
| `AiNarrative` | `string(5000)?` | No | Convenience field duplicating narrative from snapshot |
| `Notes` | `string(1000)?` | No | User-added notes |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Indexes**: `(ProjectId, ReportType, PeriodStart)`, `(ProjectId, Status)`
**Status lifecycle**: Draft → Final → Archived (forward-only)

---

### ReportTemplate

Defines the structure, section ordering, and section visibility for a report type. Can be project-specific or global.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int?` | No | null = global template |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly, Executive |
| `Name` | `string(100)` | Yes | e.g. "Standard Daily Report" |
| `Description` | `string(500)?` | No | |
| `IsDefault` | `bool` | Yes | Default template for this type per project |
| `LayoutJson` | `string` | Yes | Section ordering and layout config as JSON array |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint**: Filtered unique index on `(ProjectId, ReportType)` WHERE `IsDefault = 1`

---

### ReportSection

A section within a report instance, derived from the data snapshot at generation time.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ReportInstanceId` | `Guid` | Yes | FK → ReportInstance |
| `SectionType` | `ReportSectionType` | Yes | Text, Table, Chart, Image, AiNarrative |
| `Title` | `string(200)` | Yes | Section heading |
| `OrderIndex` | `int` | Yes | Display ordering |
| `ContentJson` | `string` | Yes | Section data serialized to JSON |
| `CreatedAt` | `DateTime` | Yes | |

---

### ReportSchedule

A scheduled generation configuration per project and report type.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly |
| `TemplateId` | `Guid?` | No | FK → ReportTemplate |
| `Frequency` | `ScheduleFrequency` | Yes | Daily, Weekly, Monthly |
| `DayOfWeek` | `int?` | No | 0=Sunday..6=Saturday (for Weekly) |
| `DayOfMonth` | `int?` | No | 1..31 (for Monthly) |
| `TimeOfDay` | `TimeSpan` | Yes | Generation time in configured timezone |
| `TimeZoneId` | `string(100)` | Yes | IANA or Windows timezone ID (default UTC) |
| `ExportFormats` | `string` | Yes | JSON array of format strings |
| `IsActive` | `bool` | Yes | |
| `LastRunAt` | `DateTime?` | No | Most recent generation attempt start (UTC) |
| `LastStatus` | `string(20)?` | No | "Success", "Failed", or "Skipped" |
| `LastErrorMessage` | `string(2000)?` | No | Exception message from last failure |
| `LastSuccessfulRunAt` | `DateTime?` | No | Timestamp of last successful generation (UTC) |
| `RetryCount` | `int` | Yes | Consecutive failures since last success |
| `MaxRetries` | `int` | Yes | Max before auto-deactivation (default 3) |
| `NextRunAt` | `DateTime` | Yes | Pre-computed next scheduled run in UTC |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint**: `(ProjectId, ReportType)` — one schedule per type per project

---

### ReportExport

A tracking record for each export operation.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ReportInstanceId` | `Guid` | Yes | FK → ReportInstance |
| `Format` | `ExportFormat` | Yes | Excel, Pdf, Word |
| `FilePath` | `string(1000)` | Yes | Absolute path to exported file |
| `FileSizeBytes` | `long` | Yes | |
| `ExportedAt` | `DateTime` | Yes | |
| `ExportedBy` | `string(100)?` | No | |

---

### ReportSettings

Per-project, per-report-type configuration for section visibility.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `ReportType` | `ReportType` | Yes | Daily, Weekly, Monthly, Executive |
| `EnabledSectionsJson` | `string` | Yes | JSON array of enabled section identifiers |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint**: `(ProjectId, ReportType)` — one settings row per report type per project

---

### ProjectParty

A party involved in a project for report headers and footers.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project |
| `Role` | `PartyRole` | Yes | Client, MainContractor, SubContractor |
| `Name` | `string(200)` | Yes | Legal name |
| `LogoPath` | `string(1000)?` | No | Path to uploaded logo |
| `Address` | `string(500)?` | No | |
| `ContactPerson` | `string(100)?` | No | |
| `ContactEmail` | `string(100)?` | No | |
| `ContactPhone` | `string(50)?` | No | |
| `DisplayOrder` | `int` | Yes | Ordering for Sub Contractors |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Unique constraint**: Filtered unique index on `(ProjectId, Role)` WHERE `Role IN (0, 1)` — at most one Client and one MainContractor per project

---

## Enums

| Enum | Values |
|------|--------|
| `ReportType` | Daily, Weekly, Monthly, Executive |
| `ReportStatus` | Draft, Final, Archived |
| `ReportSectionType` | Text, Table, Chart, Image, AiNarrative |
| `ScheduleFrequency` | Daily, Weekly, Monthly |
| `ExportFormat` | Excel, Pdf, Word |
| `PartyRole` | Client, MainContractor, SubContractor |

---

## Canonical Section Identifiers

Defined as constants on `ReportSectionIds` in `Planova.Reporting.Domain`:

| Identifier | Applies To |
|-----------|------------|
| `ProjectInfo` | All |
| `ProgressToday` | Daily |
| `Workforce` | Daily |
| `Equipment` | Daily |
| `Issues` | Daily |
| `ProgressByWbs` | Weekly, Monthly |
| `ResourceUsage` | Weekly |
| `Delays` | Weekly |
| `LookAhead` | Weekly |
| `EvmSummary` | Monthly |
| `SCurve` | Monthly, Executive |
| `BudgetVsActual` | Monthly |
| `ResourceProductivity` | Monthly |
| `KpiDashboard` | Executive |
| `RiskHighlights` | Executive |
| `FinancialOverview` | Executive |
| `MilestoneStatus` | Executive |
| `SubContractors` | All |
| `ExecutiveSummary` | Executive |
| `Photos` | Daily, Weekly |
| `AiNarrative` | All |

---

## Relationships

- **ReportInstance** → **ReportTemplate**: Many-to-One (optional)
- **ReportInstance** → **ReportSection**: One-to-Many
- **ReportInstance** → **ReportExport**: One-to-Many
- **ReportSchedule** → **ReportTemplate**: Many-to-One (optional)
- **ReportSchedule** → **Project**: Many-to-One
- **ReportInstance** → **Project**: Many-to-One
- **ReportSettings** → **Project**: Many-to-One
- **ProjectParty** → **Project**: Many-to-One

---

## Validation Rules

- `ReportInstance.Status` transitions: Draft → Final → Archived (forward-only)
- `ReportSchedule.Frequency`: Daily requires no day-of-week/month; Weekly requires DayOfWeek; Monthly requires DayOfMonth
- `ReportSchedule.NextRunAt` must be recomputed on every schedule update or tick
- `ReportInstance.DataSnapshotJson` and `AiNarrative` must be written atomically (same transaction)
- `ProjectParty`: at most one Client and one MainContractor per project (enforced by filtered unique index)
- `ReportSettings.EnabledSectionsJson`: must contain valid section identifiers from `ReportSectionIds`
- `ReportTemplate.IsDefault`: only one default per (ProjectId, ReportType) combination
