# PHASE 004 – PLANOVA EXCEL INTEGRATION
# Enterprise Master Implementation Plan

Version: 1.0
Platform: Planova Desktop
Framework: .NET 8 + WPF + MVVM
Status: Approved

---

# 1. Executive Overview

## Purpose

Provide enterprise-grade Excel integration for Planova.

Excel is used only for:

- Data import
- Data export
- Workbook browsing
- Worksheet preview
- Mapping external data to Planova entities

## Non Goals

The following are explicitly out of scope:

- Excel replacement
- Spreadsheet engine
- Formula engine
- Workbook editing
- Macro execution
- Embedded Microsoft Excel UI

## Core Principle

Database is the source of truth.

Excel is an integration format only.

---

# 2. Business Requirements

BR-001 Import Projects

BR-002 Import Activities

BR-003 Import Resources

BR-004 Import Cost Data

BR-005 Import Risk Registers

BR-006 Preview workbook before import

BR-007 Validate data before commit

BR-008 Export entities to Excel

BR-009 Save reusable mappings

BR-010 Support English and Arabic

---

# 3. Functional Requirements

## Workbook Browser

Open workbook

Display worksheets

Display metadata

Display row counts

Display column counts

Preview selected worksheet

## Import Wizard

Select file

Select worksheet

Detect columns

Map fields

Validate records

Preview records

Commit import

## Export Wizard

Select entity

Select columns

Configure export

Generate workbook

Save workbook

## Mapping Profiles

Create profile

Edit profile

Delete profile

Clone profile

Reuse profile

---

# 4. Non Functional Requirements

.NET 8

Async First

CancellationToken Support

RTL Support

Localization Support

Structured Logging

Memory Efficient

Virtualized UI

Test Coverage > 80%

---

# 5. Technology Stack

## Core

.NET 8

C# 12

WPF

CommunityToolkit.Mvvm

Microsoft.Extensions.DependencyInjection

Serilog

SQLite

## Excel Libraries

Primary:
ClosedXML

Secondary:
EPPlus

Avoid:
Microsoft.Office.Interop.Excel

Reason:
No Excel installation dependency

---

# 6. Architecture

Planova.UI
    ↓
Planova.Excel
    ↓
Planova.Persistence
    ↓
Planova.Domain

## Architecture Rules

No business logic in Views

No code-behind business logic

Dependency Injection only

MVVM only

---

# 7. Module Catalog

Planova.Excel

Services

Readers

Writers

Validation

Mapping

Models

Extensions

---

# 8. Project Structure

Planova.Excel

Services

Readers

Writers

Models

Validation

Mapping

Extensions

Planova.UI

Views/Excel

ViewModels/Excel

Tests

Excel.Tests

---

# 9. Domain Models

WorkbookInfo

WorksheetInfo

PreviewData

ImportRequest

ImportResult

ExportRequest

ExportResult

MappingProfile

ValidationResult

ValidationError

---

# 10. Database Strategy

Table:
ExcelMappingProfiles

Columns:

Id

Name

EntityType

Version

DefinitionJson

CreatedAt

ModifiedAt

DeletedAt

IsDeleted

## Audit

Required

## Soft Delete

Required

---

# 11. Format Support Matrix

XLSX

Read: Yes

Write: Yes

Preview: Yes

Import: Yes

Export: Yes

XLSM

Read: Yes

Write: No

Preview: Yes

Import: Yes

Export: No

XLS

Read: Optional

Write: No

Preview: Optional

Import: Optional

Export: No

Unsupported:

Password Protected Files

Macro Execution

Embedded Objects

---

# 12. Workbook Browser Specification

Features

Workbook metadata

Worksheet navigation

Preview grid

Search

Pagination

Virtualization

Read-only mode

No editing

---

# 13. Import Engine Specification

Step 1

Select workbook

Step 2

Load workbook

Step 3

Select worksheet

Step 4

Detect headers

Step 5

Apply mapping

Step 6

Validate

Step 7

Preview

Step 8

Commit

---

# 14. Export Engine Specification

Select entity

Select fields

Configure output

Generate workbook

Save workbook

Open location

---

# 15. Mapping Profile Specification

Fields

Profile Name

Entity Type

Version

Column Mappings

Validation Rules

Created Date

Modified Date

Example

Activity ID → Code

Activity Name → Name

Start Date → Start

Finish Date → Finish

---

# 16. Validation Engine

Required Field Validation

Data Type Validation

Range Validation

Duplicate Detection

Reference Validation

Business Rule Validation

---

# 17. Security Model

Never execute VBA

Ignore macros

Remove external links

Validate extensions

Validate workbook structure

Protect against formula injection

Ignore embedded objects

---

# 18. Localization

Languages

English

Arabic

Requirements

Runtime switching

RTL support

Resource files only

No hardcoded strings

---

# 19. UI/UX Specification

Pages

Workbook Browser

Import Wizard

Export Wizard

Mapping Profiles

## Preview Grid

Read Only

Virtualized

Sortable

Filterable

Searchable

---

# 20. ViewModel Design

WorkbookBrowserViewModel

ImportViewModel

ExportViewModel

MappingProfilesViewModel

---

# 21. Service Contracts

IWorkbookReader

IWorkbookWriter

IWorkbookPreviewService

IImportService

IExportService

IMappingProfileService

IValidationService

---

# 22. Dependency Injection

Register all services through:

ServiceCollectionExtensions

Single registration location

No manual service construction

---

# 23. Error Handling Standard

Log error

Classify error

Return friendly message

Capture stack trace

Never swallow exceptions

---

# 24. Performance Strategy

Targets

Workbook Load < 1 second

Preview 1000 rows < 1 second

Import 10000 rows < 2 seconds

Export 10000 rows < 2 seconds

Memory Budget < 500 MB

---

# 25. Large File Strategy

Background processing

CancellationToken

Progress reporting

Streaming where possible

Virtualized UI

---

# 26. Testing Strategy

Unit Tests

Workbook Reader

Workbook Writer

Validation Engine

Mapping Profiles

Integration Tests

Import Workflow

Export Workflow

Database Persistence

UI Tests

Import Wizard

Export Wizard

Workbook Browser

---

# 27. Edge Case Matrix

Corrupted workbook

Empty workbook

Empty worksheet

Missing headers

Duplicate headers

Duplicate rows

Invalid data types

Large files

Localization switching

RTL rendering

---

# 28. Coding Standards

Async/Await

CancellationToken

MVVM

Dependency Injection

Nullable Enabled

XML Documentation

Structured Logging

SOLID Principles

Clean Architecture

---

# 29. Code Review Checklist

Architecture compliance

Naming standards

Null safety

Localization compliance

Performance compliance

Security compliance

Test coverage compliance

---

# 30. Risk Register

Risk:
Invalid customer workbooks

Mitigation:
Validation engine

Risk:
Huge files

Mitigation:
Virtualization

Risk:
Data mismatch

Mitigation:
Mapping profiles

Risk:
Localization defects

Mitigation:
RTL testing

---

# 31. Implementation Roadmap

Phase A

Models

Contracts

Infrastructure

Phase B

Workbook Reader

Workbook Browser

Phase C

Preview UI

Phase D

Import Engine

Phase E

Export Engine

Phase F

Mapping Profiles

Phase G

Validation

Phase H

Localization

Phase I

Testing

Phase J

Documentation

---

# 32. Deliverables

Planova.Excel Project

Workbook Browser

Worksheet Preview

Import Wizard

Export Wizard

Mapping Profiles

Validation Engine

Localization

Automated Tests

Documentation

---

# 33. Definition Of Done

Import workflow completed

Export workflow completed

Preview workflow completed

Localization completed

Security requirements completed

Performance targets achieved

Tests passing

Documentation completed
