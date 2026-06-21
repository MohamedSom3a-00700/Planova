# Data Model: Overall Enhancement Plan

## Entity Relationship Summary

```
Project (1) ──has many──> Party (M) ──through──> ProjectParty (M)
Project (1) ──has many──> BoqImportSession (M)
Project (1) ──has many──> Boq (M) ──has many──> BoqItem (M)
Project (1) ──has many──> Wbs (M) ──has many──> WbsItem (self-referencing)
Wbs (1) ──has one──> WbsEditLock (0..1)
ProjectFolder (1) ──has many──> ProjectSubfolder (M)  [auto-created on disk]
```

## Project (Enhanced)

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Unique identifier (existing) |
| Name | string | Project name (existing) |
| ProgressPercentage | decimal | Overall progress 0-100% |
| HealthIndicator | ProjectHealth | Green/Yellow/Red health status |
| CurrentDataDate | DateTime? | Latest schedule data date |
| Budget | decimal? | Current approved budget |
| ActivityCount | int | Number of schedule activities |
| LastUpdatedDate | DateTime | Last modification timestamp |
| LogoPath | string? | Path to project logo image |
| CoverImagePath | string? | Path to project cover image |
| ConnectedXerPath | string? | Path to connected XER file |
| ConnectedDatabase | string? | Connected database name/connection |
| LastImportDate | DateTime? | Last XER import timestamp |
| LastExportDate | DateTime? | Last export timestamp |
| ProjectFolderPath | string? | Root folder path for auto-created subfolders |
| GoogleMapsLink | string? | Stored Google Maps URL |
| Latitude | double? | Extracted map latitude |
| Longitude | double? | Extracted map longitude |
| OriginalBudget | decimal? | Baseline budget at project start |
| CurrentBudget | decimal? | Current (revised) budget |
| ActualCost | decimal? | Total cost incurred to date |
| EarnedValue | decimal? | EV = %complete * budget |
| Cpi | decimal? | Cost Performance Index (EV/AC) |
| Spi | decimal? | Schedule Performance Index (EV/PV) |
| ScheduleHealthPct | decimal? | Schedule health percentage 0-100% |
| CostHealthPct | decimal? | Cost health percentage 0-100% |
| RiskLevel | ProjectRiskLevel | Low/Medium/High risk assessment |
| CriticalActivityCount | int? | Number of critical-path activities |

**Validation rules**: ProgressPercentage 0-100; HealthIndicator required; Budget values non-negative; CPI/SPI computed (not directly settable).

## ProjectHealth (enum)

Green, Yellow, Red

## ProjectRiskLevel (enum)

Low, Medium, High

## Party

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Organization name |
| PartyType | PartyType | Client, MainContractor, Subcontractor, Consultant |
| LogoPath | string? | Path to organization logo |
| ContactName | string? | Primary contact person |
| ContactEmail | string? | Contact email address |
| ContactPhone | string? | Contact phone number |
| Address | string? | Physical address |
| Notes | string? | Optional notes |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**Validation rules**: Name required (max 200 chars); PartyType required; Email validated if provided.

## PartyType (enum)

Client, MainContractor, Subcontractor, Consultant

## ProjectParty

| Field | Type | Description |
|-------|------|-------------|
| ProjectId | int | FK to Project |
| PartyId | Guid | FK to Party |
| Role | string | Role description within this project |

**Validation rules**: Unique constraint on (ProjectId, PartyId).

## BoqImportSession

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ProjectId | int | FK to Project |
| FileName | string | Original Excel file name |
| FilePath | string | Stored file path |
| TotalSheetsDetected | int | Number of BOQ worksheets found |
| TotalSheetsImported | int | Number actually imported |
| TotalSectionsCreated | int | Sections created (merge mode) |
| TotalItemsImported | int | Total BOQ items imported |
| TotalAmount | decimal | Sum of all amounts |
| ImportMode | ImportMode | Single, Multiple, Merge |
| ImportedAt | DateTime | Import timestamp |
| ImportedByUserId | int? | Who performed import |
| Status | ImportStatus | Completed, Partial, Failed |

**Validation rules**: ProjectId required; TotalItemsImported >= 0; TotalAmount >= 0.

## ImportMode (enum)

Single, Multiple, Merge

## ImportStatus (enum)

Completed, Partial, Failed

## BoqWorksheetMapping

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ImportSessionId | Guid | FK to BoqImportSession |
| WorksheetName | string | Original worksheet name (SourceSheet) |
| SortOrder | int | Original worksheet order |
| MatchConfidence | decimal | Column detection confidence 0-1 |
| ColumnMappings | string | Serialized column mapping (JSON) |
| RowsImported | int | Row count for this sheet |
| SectionAmount | decimal | Total amount for this section |

## WbsEditLock

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| WbsId | Guid | FK to Wbs (unique: one lock per WBS) |
| LockedByUserId | int | User holding the lock |
| LockedAt | DateTime | When lock was acquired |
| LockExpiresAt | DateTime | Auto-release time (e.g., 30min timeout) |

**Validation rules**: One active lock per WbsId; LockExpiresAt > LockedAt; LockExpiresAt auto-extended on user activity.

## Wbs (Enhanced from existing)

New/enhanced fields beyond the existing Wbs entity:

| Field | Type | Description |
|-------|------|-------------|
| Status | WbsStatus | Draft, UnderReview, Approved, Archived |
| Source | WbsSource | BOQ, Template, AI, Manual, Imported, Primavera |
| TotalWeight | decimal | Computed sum of top-level item weights |
| Revision | int | Incremented on status changes |

**WbsStatus transitions**: Draft → UnderReview → Approved → Archived; Draft → Archived; UnderReview → Draft; Approved → UnderReview.

## WbsItem (Enhanced from existing)

New/enhanced fields beyond the existing WbsItem entity:

| Field | Type | Description |
|-------|------|-------------|
| Owner | string? | Responsible owner/role |
| Discipline | string? | Engineering discipline |
| Deliverable | string? | Deliverable description |
| Notes | string? | Optional notes |
| Weight | decimal? | Percentage weight (redistributed across siblings) |

## WbsTemplate (Enhanced)

New template categories from spec FR-043: Building, Infrastructure, Landscape, Roads, WaterNetwork, SewerNetwork, Industrial, PowerPlant.

## ProjectSubfolder (auto-created on disk, not persisted in DB)

Standard folders created when Project.ProjectFolderPath is set:
1. Contract
2. BOQ
3. Drawing
4. Specification
5. Schedule
6. Claim
7. Reports
8. Correspondence
9. Photo
10. Meeting & Presentations
11. Archive
12. Lookahead
13. Logos
