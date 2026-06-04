# Data Model: Excel Integration

## Entity Relationship Summary

```
Workbook (1) ──has many──> Worksheet (1) ──imported via──> ImportRequest
Worksheet (1) ──previewed as──> PreviewData
ImportRequest (1) ──produces──> ValidationResult
ImportRequest (1) ──produces──> ImportResult
ExportRequest (1) ──produces──> ExportResult
MappingProfile (1) ──applied to──> ImportRequest
```

## WorkbookInfo

| Field | Type | Description |
|-------|------|-------------|
| FilePath | string | Full path to the Excel file |
| FileSize | long | File size in bytes |
| Worksheets | List\<WorksheetInfo> | Collection of worksheets in the workbook |
| IsValid | bool | Whether the workbook structure passed validation |
| Format | string | File format (e.g., "xlsx", "xlsm") |

## WorksheetInfo

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Worksheet name |
| RowCount | int | Number of data rows |
| ColumnCount | int | Number of columns |
| Columns | List\<string> | Column header names (detected from first row) |

## PreviewData

| Field | Type | Description |
|-------|------|-------------|
| WorksheetName | string | Source worksheet name |
| Columns | List\<string> | Column header names |
| Rows | List\<List\<object>> | Preview data rows (first N rows, configurable page size) |
| TotalRowCount | int | Total rows available in the worksheet |
| PageSize | int | Number of rows per preview page |
| CurrentPage | int | Current page index |

## ImportRequest

| Field | Type | Description |
|-------|------|-------------|
| WorksheetName | string | Source worksheet |
| EntityType | string | Target Planova entity type (Project, Activity, Resource, Cost, Risk) |
| ColumnMappings | Dictionary\<string, string> | Excel column → Planova field mappings |
| MappingProfileId | Guid? | Optional profile to apply |
| BatchSize | int | Rows per commit batch (configurable, default 1000) |
| DuplicateHandling | DuplicateStrategy | How to handle matching records (Update, Skip, Prompt) |

**Validation rules**: EntityType must be a known Planova entity; ColumnMappings must cover all required fields; BatchSize must be between 1 and 10000.

**State transitions**: Draft → Validated (after validation) → Committed (after successful batch commit) or Failed (on error)

## ImportResult

| Field | Type | Description |
|-------|------|-------------|
| TotalRecords | int | Total rows processed |
| ImportedRecords | int | Rows successfully imported |
| UpdatedRecords | int | Existing records updated |
| SkippedRecords | int | Rows skipped (duplicates, invalid) |
| FailedRecords | int | Rows that failed validation |
| Errors | List\<ValidationError> | Per-row validation errors |
| CompletedBatches | int | Number of batches committed |
| TotalBatches | int | Total batches to process |
| Duration | TimeSpan | Total processing time |

## ExportRequest

| Field | Type | Description |
|-------|------|-------------|
| EntityType | string | Planova entity type to export |
| SelectedColumns | List\<string> | Fields to include in the export |
| OutputPath | string | Where to save the generated workbook |
| SheetName | string | Worksheet name for the output (default: entity type name) |
| IncludeHeaders | bool | Whether to include column headers (default: true) |

**Validation rules**: EntityType must be a known Planova entity; SelectedColumns must contain at least one field; OutputPath must be writable.

## ExportResult

| Field | Type | Description |
|-------|------|-------------|
| TotalRecords | int | Records exported |
| OutputPath | string | Path to the generated workbook |
| FileSize | long | Output file size in bytes |
| Duration | TimeSpan | Total processing time |
| Success | bool | Whether the export completed without errors |

## MappingProfile

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | User-defined profile name |
| EntityType | string | Target Planova entity type |
| Version | int | Profile version (incremented on edits) |
| ColumnMappings | Dictionary\<string, string> | Excel column → Planova field mappings |
| ValidationRules | List\<string> | Optional custom validation rule identifiers |
| CreatedAt | DateTime | Creation timestamp |
| ModifiedAt | DateTime | Last modification timestamp |

**Persistence**: Stored in EF Core table `ExcelMappingProfiles` with JSON serialization of ColumnMappings and ValidationRules. Uses soft delete (IsDeleted, DeletedAt).

## ValidationResult

| Field | Type | Description |
|-------|------|-------------|
| IsValid | bool | Whether all records passed validation |
| TotalErrors | int | Total validation errors found |
| Errors | List\<ValidationError> | Collection of validation errors |
| Warnings | List\<string> | Non-blocking warnings |

## ValidationError

| Field | Type | Description |
|-------|------|-------------|
| RowIndex | int | Row number in the source data |
| ColumnName | string | Column where the error occurred |
| ErrorType | string | Error category (Required, DataType, Range, Duplicate, Reference, BusinessRule) |
| Message | string | User-friendly error description |
| RawValue | object | Original value that failed validation |

## DuplicateStrategy (enum)

| Value | Behavior |
|-------|----------|
| Prompt | Show user the detected duplicates and ask how to handle each (default) |
| UpdateAll | Automatically update all matching existing records |
| SkipAll | Automatically skip all matching records, import only new ones |
| Cancel | Abort the import if any duplicates are detected |
