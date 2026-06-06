---
name: planova-development
description: Use when implementing, editing, or reviewing C# code in the Planova project. Guides .NET 8/C# 12 conventions, Clean Architecture layering, MVVM patterns, EF Core persistence, spec-first workflow, module structure, and testing standards. Use ONLY for Planova project code, not for general coding questions.
---

# Planova Development Skill

This skill guides agents on Planova's project-specific conventions, architecture, and workflows.

## Project Overview

Planova is a **.NET 8 / WPF** desktop-first enterprise project controls platform built with Clean Architecture.

**Solution**: `Planova.slnx` with main modules:
- `Planova.UI` — WPF shell, views, viewmodels
- `Planova.Application` — application services, use cases
- `Planova.Domain` — domain entities, enums, interfaces
- `Planova.Infrastructure` — external concerns
- `Planova.Persistence` — EF Core DbContext, migrations, repositories
- `Planova.Excel` — Excel import/export (ClosedXML, EPPlus)
- `Planova.Localization` — .resx localization (English + Arabic, RTL)
- `Planova.Shared` — cross-cutting contracts, base classes

## Development Workflow: Spec-First

1. Read the spec: `specs/{phase}/spec.md`
2. Read contracts: `specs/{phase}/contracts/`
3. Read data model: `specs/{phase}/data-model.md`
4. Implement order: **Domain → Persistence → Application → UI**
5. Verify against: `specs/{phase}/checklists/requirements.md`
6. Run tests before committing

## C# 12+ Conventions

- File-scoped namespaces (`namespace Planova.Cost;`)
- Primary constructors for simple classes
- Collection expressions (`[]` instead of `new List<T>()`)
- `_camelCase` for private fields
- Async methods suffixed with `Async`
- `ArgumentNullException.ThrowIfNull` for null checks
- `readonly` fields where possible
- `global using` in `GlobalUsings.cs` per project

## Clean Architecture Layers

### Domain (`Planova.Domain`)
- No dependencies on other projects
- Entities: record types or sealed classes, `BaseEntity<int>` with `Id`, `CreatedAt`, `UpdatedAt`
- Value objects: `readonly record struct`
- Enums: Smart Enums for complex state
- Repository interfaces: `I{Entity}Repository`

### Persistence (`Planova.Persistence`)
- `DbContext` per bounded context (e.g. `CostDbContext`)
- Entity configurations via `IEntityTypeConfiguration<T>` in `Configurations/`
- Migrations per context: `Add-Migration -Context {Name}DbContext`
- Never expose `IQueryable` beyond repository

### Application (`Planova.Application`)
- Services: `I{Feature}Service` → `{Feature}Service`
- Methods return `Result<T>` (from `Planova.Shared`)
- No AutoMapper — use extension methods or manual mapping
- FluentValidation for complex inputs

### UI (`Planova.UI`)
- CommunityToolkit.Mvvm: `[ObservableProperty]`, `[RelayCommand]`
- Data templates in `App.xaml` or module resource dictionaries
- All UI text via `Planova.Localization.Resources` — never hardcode strings
- RTL: `FlowDirection` binding for Arabic support

## EF Core / SQLite

- Code First migrations
- Primary key: `Id` column, `int` default, `Guid` where specified
- Fluent API in entity configuration classes
- Cascade delete disabled by default
- `AsNoTracking()` for read queries
- Database path: `{AppData}/Planova/planova.db`

## Testing

- xUnit + FluentAssertions + Moq
- Naming: `{MethodName}_Should_{Expected}_When_{Condition}`
- Arrange → Act → Assert with blank line separation
- Repository tests use SQLite in-memory

## Each "Studio" Module Pattern

All Studio modules (BOQ, WBS, Activity, Resource, Cost) follow:
1. Domain entities, value objects, enums, repository interfaces
2. EF Core entity configuration and DbContext
3. Application services with CRUD + domain-specific operations
4. ViewModels for list/detail views, dialogs
5. WPF views with Fluent UI WPF controls
6. Reports with QuestPDF
7. Localization resources
8. Unit tests

Cross-module communication through `Planova.Shared` interfaces. No direct project references between Studio modules.

## AI Integration (Semantic Kernel)

- Framework: Semantic Kernel
- Provider abstraction via `IAIProvider`
- Default: Ollama + Llama 3.2
- Never hardcode model names; read from configuration
