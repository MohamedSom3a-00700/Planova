# Research: Project Management Foundation

**Phase**: 0 — Outline & Research

**Date**: 2026-06-01

## Technology Decisions

### Decision: Entity Design Pattern
- **Decision**: Use plain POCO entities with auto-properties in Planova.Domain.Entities, matching existing SchemaVersion and UserPreferences patterns.
- **Rationale**: Consistency with existing codebase. The current entities are simple data carriers without behavior. Adding domain behavior (factory methods, validation) can be introduced later when needed without breaking changes.
- **Alternatives considered**: Rich domain model with encapsulated behavior — deferred to avoid over-engineering for CRUD-first entities.

### Decision: Status Lifecycle Implementation
- **Decision**: Implement project status as a dedicated `ProjectStatus` value object class with a fixed set of seven states (Draft, Under Review, Approved, In Progress, On Hold, Completed, Cancelled) and a transition map that validates allowed moves.
- **Rationale**: A value object prevents invalid status strings from entering the system and centralizes transition rules. The transition map is straightforward to test and extend.
- **Alternatives considered**: Simple string enum — rejected because it allows invalid transitions and scatters validation logic.

### Decision: Uniqueness Enforcement
- **Decision**: Enforce uniqueness at the database level via unique indexes on Project.Code, Client.Code, Client.Name, Contract.Number, with EF Core configuration. Application-layer pre-check before save for user-friendly error messages.
- **Rationale**: Database-level enforcement guarantees integrity even if application validation is bypassed. Dual-layer (app + DB) provides the best UX.
- **Alternatives considered**: Application-only validation — rejected due to race conditions and data integrity risk.

### Decision: Dashboard Data Aggregation
- **Decision**: Dashboard queries are served by dedicated dashboard query services in the Application layer that aggregate directly via EF Core LINQ queries (counts by status, recent activity by last-modified timestamp).
- **Rationale**: Simple aggregation queries are efficient with SQLite at 1,000 record scale. No need for a separate read model or caching layer.
- **Alternatives considered**: CQRS read model — rejected as over-engineering for Phase 1 data volume.

### Decision: Reporting Approach
- **Decision**: Use lightweight in-memory data aggregation with WPF list/grid views and print-friendly styling. Export via QuestPDF (existing infrastructure dependency in plan) for PDF generation.
- **Rationale**: QuestPDF is already in the approved technology stack. In-memory aggregation is sufficient for 1,000 records.
- **Alternatives considered**: Dedicated reporting engine — rejected per constitution Build vs Buy guidance.

### Decision: Workspace Integration Pattern
- **Decision**: Follow the existing ShellViewModel.RegisterNavigationTargets pattern. New workspaces (Projects, Clients, Contracts, Dashboard, Reports, Profile) register as navigation targets that create their ViewModel-bound views.
- **Rationale**: Matches existing navigation infrastructure. No new patterns needed.
- **Alternatives considered**: Separate navigation service per module — rejected for consistency with existing approach.

### Decision: Localization Extension
- **Decision**: Add new RESX resource files for the Project Management domain (Projects, Clients, Contracts, Dashboard, Reports, Profile labels) in Planova.Localization.Resources, following the existing English/Arabic pair pattern.
- **Rationale**: Maintains the existing RESX-based localization pattern. The existing LocalizationService.GetString() method is already consumed by all views.
- **Alternatives considered**: Database-driven localization — rejected for complexity at this scale.

### Decision: Repository Pattern Justification
- **Decision**: Use repository interfaces in Application layer and EF Core implementations in Persistence layer for Project, Client, and Contract entities.
- **Rationale**: Per the constitution, repository pattern is "prohibited unless justified by a documented architectural decision." The justification here is Clean Architecture boundaries: Domain entities must be loaded by the Application layer without depending on EF Core. Repositories provide this abstraction. A simpler alternative (e.g., DbSet directly in Application) would violate the dependency inversion principle.
- **Alternatives considered**: Direct DbSet usage from Application — rejected because it would create a direct dependency on EF Core in the Application layer, violating Clean Architecture.

## Dependency Analysis

| Dependency | Pattern | Notes |
|------------|---------|-------|
| Phase 0 Shell | Extension / Add-to-existing | All new views register into existing ShellViewModel navigation |
| Phase 0 DbContext | Extension / Add-to-existing | New entity configurations registered in OnModelCreating |
| Phase 0 Localization | Extension / Add-resources | New RESX files added alongside existing ones |
| Phase 0 Services (DI) | Extension / Register | New services registered in the existing DI container |
