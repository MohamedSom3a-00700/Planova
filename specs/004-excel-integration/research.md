# Research: Excel Integration

## Language & Platform

- **Decision**: .NET 8, C# 12, WPF Desktop
- **Rationale**: Approved technology stack per Planova Constitution (Section: Technology Standards)
- **Alternatives considered**: .NET Framework (rejected — end-of-life), MAUI (rejected — not yet stable for desktop)

## Excel Libraries

- **Decision**: ClosedXML (primary), EPPlus (secondary)
  - ClosedXML for read/write of .xlsx files (open-source, MIT license)
  - EPPlus for legacy format support and advanced import scenarios
- **Rationale**: No Excel installation dependency; both are mature, well-maintained libraries
- **Alternatives considered**: Microsoft.Office.Interop.Excel (rejected — requires Excel installation), Open XML SDK (rejected — too low-level, no benefit over ClosedXML)

## Architecture

- **Decision**: Clean Architecture with dedicated Planova.Excel module
- **Rationale**: Constitution mandates Clean Architecture; Excel as Infrastructure layer keeps Domain pure
- **Key design**: Planova.Excel references Planova.Persistence (saving mapping profiles) and Planova.Domain (entity contracts); Planova.UI references Planova.Excel for ViewModels

## MVVM & DI

- **Decision**: CommunityToolkit.Mvvm with Dependency Injection via Microsoft.Extensions.DependencyInjection
- **Rationale**: Constitution mandates MVVM; DI is required in all Planova modules
- **Service registration**: Single `ServiceCollectionExtensions.AddPlanovaExcel()` in Planova.Excel

## Validation Engine

- **Decision**: Custom validation service with pluggable validators per entity type
- **Rationale**: Each Planova entity (project, activity, resource, cost, risk) has different validation rules; pluggable pattern keeps validation maintainable
- **Validators**: Required fields, data types, ranges, duplicates, reference integrity, business rules

## Mapping Profiles

- **Decision**: JSON-based profile definitions stored in SQLite via EF Core
- **Rationale**: JSON is portable and easily versioned; EF Core aligns with existing data access patterns
- **Schema**: ExcelMappingProfiles table with columns: Id, Name, EntityType, Version, DefinitionJson, CreatedAt, ModifiedAt, DeletedAt, IsDeleted (soft delete)

## Localization

- **Decision**: Runtime language switching with .resx resource files; RTL layout support
- **Rationale**: Constitution mandates Multilingual First; Runtime switching is specified in FR-011

## Large File Handling

- **Decision**: Stream-based reading, virtualized UI (DataGrid virtualization), background processing with CancellationToken and progress reporting
- **Rationale**: Spec requires handling 10,000+ rows without blocking the UI; virtualized UI is standard WPF best practice for large datasets

## Security

- **Decision**: Never execute macros/VBA, validate file extensions and workbook structure, strip external links, protect against formula injection
- **Rationale**: Excel files from external sources may contain malicious content; all files are treated as untrusted until validated
