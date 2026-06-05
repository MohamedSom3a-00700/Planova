# Feature Specification: Excel Integration

**Feature Branch**: `004-excel-integration`

**Created**: 2026-06-03

**Status**: Draft

**Input**: User description: "Excel integration with import, export, workbook browsing, and mapping profiles"

## User Scenarios & Testing

### User Story 1 - Import Data from Excel to Planova (Priority: P1)

A project manager or administrator needs to import project plans, activities, resources, and costs from an Excel workbook into Planova. They open the Import Wizard, select their Excel file, map columns to Planova entities, validate the data, and commit the import. Viewers can browse and preview workbooks but cannot import or export.

**Why this priority**: Importing data is the primary integration use case — users need to bring existing project data into Planova.

**Independent Test**: Can be tested by importing a valid Excel workbook with project data and verifying all records appear in Planova.

**Acceptance Scenarios**:

1. **Given** a user has a valid Excel workbook, **When** they select it in the Import Wizard and map columns to Planova entities, **Then** the system validates the data and displays a preview before commit
2. **Given** a user reviews the import preview, **When** they confirm the import, **Then** all valid records are committed to Planova and the user sees a success confirmation with record counts
3. **Given** an Excel file contains invalid data (wrong types, missing required fields), **When** validation runs, **Then** the user sees clear error messages with row-level details and can fix and retry
4. **Given** some records in the Excel file match existing Planova records by key field (e.g., project code), **When** the import preview shows detected duplicates, **Then** the user can choose to update existing records, skip duplicates, or cancel the import

---

### User Story 2 - Browse and Preview Excel Workbooks (Priority: P1)

A user needs to browse the contents of an Excel workbook before deciding what to import. They open the Workbook Browser, see all worksheets, preview their contents, and examine column headers and data.

**Why this priority**: Preview is a prerequisite for informed import decisions and is part of the core import workflow.

**Independent Test**: Can be tested by opening a workbook in the Workbook Browser and confirming worksheets are listed and preview data is displayed.

**Acceptance Scenarios**:

1. **Given** a user opens a workbook, **When** the workbook loads, **Then** they see a list of worksheets with metadata (row count, column count)
2. **Given** a user selects a worksheet, **When** the preview loads, **Then** they see a read-only grid with column headers and data rows
3. **Given** a preview grid has many rows, **When** the user scrolls, **Then** data loads smoothly with pagination

---

### User Story 3 - Export Planova Data to Excel (Priority: P2)

A user needs to export Planova entities (projects, activities, resources) to an Excel workbook for reporting or sharing with stakeholders. They select the entity type, choose columns, configure output options, and generate the workbook.

**Why this priority**: Export is a complementary feature to import, enabling data exchange with external stakeholders.

**Independent Test**: Can be tested by exporting Planova data and confirming the resulting workbook contains the expected entities and columns.

**Acceptance Scenarios**:

1. **Given** a user selects an entity type to export, **When** they choose columns and configure output options, **Then** the system generates a properly formatted Excel workbook
2. **Given** an export completes, **When** the workbook is saved, **Then** the user can open it with standard spreadsheet software

---

### User Story 4 - Save and Reuse Mapping Profiles (Priority: P3)

A user who regularly imports data from the same source wants to save their column mappings as a reusable profile. They save the mapping after a successful import and reuse it for subsequent imports.

**Why this priority**: Mapping profiles improve efficiency for recurring imports but are not required for first-time users.

**Independent Test**: Can be tested by saving a mapping profile, then importing a similar workbook and applying the saved profile automatically.

**Acceptance Scenarios**:

1. **Given** a user has completed a successful import with defined column mappings, **When** they save the mapping as a profile, **Then** the profile is stored and available for reuse
2. **Given** a user starts a new import, **When** they select a saved mapping profile, **Then** the columns are automatically mapped and the user can proceed to validation

---

### Edge Cases

- What happens when a workbook is corrupted or has an unsupported format?
- How does the system handle empty worksheets or workbooks?
- What happens when required columns are missing or headers are duplicated?
- How does the system handle very large files (thousands of rows)?
- What happens when a mapping profile references columns that don't exist in the current workbook?
- How does the system handle password-protected files or files with macros?
- What happens when imported records match existing Planova entities by key field?
- What happens when an import fails partway through — are committed batches retained or rolled back?

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow users to open and browse Excel workbooks (.xlsx, .xlsm) in read-only mode
- **FR-002**: System MUST display worksheet metadata including name, row count, and column count
- **FR-003**: System MUST render a read-only preview of selected worksheet data
- **FR-004**: System MUST support importing data from Excel worksheets into Planova entities (projects, activities, resources, costs, risks)
- **FR-005**: System MUST auto-detect column headers from Excel worksheets
- **FR-006**: System MUST allow users to map Excel columns to Planova entity fields before import
- **FR-007**: System MUST validate imported data for required fields, data types, and business rules before commit
- **FR-008**: System MUST display validation errors with row-level details and allow users to retry after fixing data
- **FR-009**: System MUST support exporting Planova entities to .xlsx workbooks with user-selected columns
- **FR-010**: System MUST allow users to save, edit, delete, clone, and reuse column mapping profiles
- **FR-011**: System MUST support both English and Arabic language interfaces with runtime switching
- **FR-012**: System MUST handle large files (10,000+ rows) without blocking the user interface
- **FR-013**: System MUST never execute macros or VBA code from opened workbooks
- **FR-014**: System MUST validate workbook structure and reject unsupported or potentially malicious files
- **FR-015**: System MUST detect when imported records match existing Planova records by key field and present the user with options to update, skip, or cancel
- **FR-016**: System MUST process imports in configurable batch sizes with atomic commit per batch (all-or-nothing per batch)

### Key Entities

- **Workbook**: An Excel file opened for browsing, preview, or import. Contains one or more worksheets with metadata.
- **Worksheet**: A single sheet within a workbook with rows and columns of data. The unit of import/export operations.
- **Import Request**: A user-initiated operation to transfer data from a worksheet into Planova entities, with defined column mappings, validation rules, and configurable batch processing with atomic commit per batch.
- **Export Request**: A user-initiated operation to extract Planova entities into an Excel workbook with selected columns and output configuration.
- **Mapping Profile**: A saved set of column-to-field mappings for a specific entity type, enabling reuse across imports.
- **Validation Result**: The outcome of validating imported data, including errors, warnings, and record counts.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can complete a data import workflow (file selection through commit) in under 5 minutes for a workbook of 1,000 rows
- **SC-002**: Workbooks up to 10 MB load for preview in under 2 seconds
- **SC-003**: The system handles import of 10,000 rows without performance degradation or timeout
- **SC-004**: Users can export 10,000 records to Excel in under 3 seconds
- **SC-005**: Validation detects and reports 100% of invalid records before any data is committed
- **SC-006**: Users with saved mapping profiles complete imports 50% faster than those creating mappings from scratch
- **SC-007**: The feature works identically in English and Arabic language modes without layout or display issues
- **SC-008**: 90% of users successfully complete their first import on the first attempt (with valid source data)

## Clarifications

### Session 2026-06-03

- Q: How should the system handle imported records that match existing Planova records? → A: Match by key field (e.g., project code, activity ID) and update existing records, with user confirmation during import preview
- Q: Should a failed import be atomic or partial? → A: Atomic per configurable batch; each batch commits fully or rolls back entirely
- Q: Which user roles can import, export, and browse Excel data? → A: Project managers and admins can import/export; viewers can only browse and preview

## Assumptions

- Users have access to valid Excel workbooks with well-structured tabular data (column headers in the first row)
- The system targets .xlsx files as the primary format; .xls support is optional due to format limitations
- The database is the authoritative data source; Excel data is always imported into the system, never edited in place
- Users have basic understanding of their data structure and can identify which columns correspond to which Planova fields
- Mapping profiles are stored locally within the application's database
- Mobile or web access to Excel integration is out of scope for the initial release
- Templates for import are created by users in their own Excel software, not provided by Planova
- The existing Planova authentication and authorization system applies to this feature (no additional security layer)
- Import and export capabilities are restricted to project managers and administrators; viewers can browse and preview only
