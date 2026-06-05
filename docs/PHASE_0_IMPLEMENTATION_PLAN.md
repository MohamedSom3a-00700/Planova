# Planova Phase 0 Implementation Plan

**Phase**: 0 - Foundation

**Date**: 2026-06-01

**Source of Truth**: [docs/01-PRODUCT_VISION.md](./01-PRODUCT_VISION.md),
[docs/04-SYSTEM_ARCHITECTURE.md](./04-SYSTEM_ARCHITECTURE.md),
[docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md),
[`docs/PLANOVA_CONSTITUTION_DRAFT.md`](./PLANOVA_CONSTITUTION_DRAFT.md),
[Planova Branding Master](./Planova%20Branding%20Master)

## Summary

Phase 0 establishes the non-negotiable foundation for Planova: the WPF shell,
navigation rail workspace, theme system, multilingual infrastructure, logging,
settings, branding system, and SQLite persistence baseline. The goal is to make
the application start cleanly, stay aligned with Clean Architecture and MVVM,
and provide the platform that every later module can plug into without rework.

This phase does not deliver business features such as BOQ, WBS, scheduling, or
claims workflows. It delivers the architectural, UX, and runtime foundation
those modules will depend on.

## Phase 0 Objectives

1. Create the solution and project structure required by the architecture.
2. Deliver the main WPF shell with Navigation Rail + Multi-Tab Workspace.
3. Implement the application bootstrap, dependency injection, and host setup.
4. Establish the dark/light theme system and Fluent UI styling baseline.
5. Apply the Planova brand system to the shell, navigation, and workspace.
6. Add English and Arabic localization with runtime language switching and RTL.
7. Configure Serilog logging and application diagnostics.
8. Add user settings and externalized configuration.
9. Create the SQLite and EF Core persistence foundation.
10. Prove the app can launch, switch language/theme, and persist settings.

## Phase 0 Scope

### In Scope

- `Planova.UI`
- `Planova.Application`
- `Planova.Domain`
- `Planova.Infrastructure`
- `Planova.Persistence`
- `Planova.Localization`
- Shared composition and contract boundaries
- App shell, navigation rail, workspace tabs
- Theme manager
- Brand asset application and visual identity pass
- Language manager
- Settings storage
- Database bootstrap and first migration foundation
- Logging pipeline

### Out of Scope

- BOQ Studio
- WBS Studio
- Activity Studio
- Resource Studio
- Cost Studio
- Reporting Center
- Primavera Studio
- Schedule Comparison Studio
- Delay Analysis Studio
- Claims Studio
- Chronology Studio
- Correspondence Center
- AI Copilot
- Knowledge Base
- Analytics Center
- Integration Hub behavior beyond shell placeholders

## Non-Negotiable Constraints

- Clean Architecture must be preserved.
- MVVM must be used for all UI composition.
- UI must remain declarative and avoid business logic.
- Dependency inversion must point inward.
- English and Arabic localization must be first-class.
- RTL support must work at runtime.
- The solution must remain automation-platform agnostic.
- The solution must remain AI-provider agnostic.
- No workflow engine, automation designer, or rule builder is introduced.
- The Phase 0 foundation must not create architecture that blocks later modular
  studio work.
- Brand identity must be established before Phase 1 feature expansion so every
  later screen inherits the same visual language.

## Target Solution Structure

The plan assumes the solution is organized around the architecture already
described in the system architecture document.

```text
Planova.sln
  Planova.UI
  Planova.Application
  Planova.Domain
  Planova.Infrastructure
  Planova.Persistence
  Planova.Localization
  Planova.Shared
```

Later domain modules may be added after the foundation is stable, but Phase 0
must leave room for them without restructuring core assemblies.

## Implementation Order

### 1. Solution and Project Bootstrap

Build the baseline solution structure first so every later task lands in the
correct layer.

Deliverables:

- `.sln` with the core projects
- project references aligned to Clean Architecture
- common analyzers and build settings
- app entrypoint and host bootstrap

Rules:

- UI may reference Application, Localization, and shared abstractions.
- Application may reference Domain and shared abstractions.
- Infrastructure and Persistence may depend on Application and Domain where
  appropriate.
- Domain remains framework-free.

### 2. Shell Window and Navigation Framework

Create the main application shell as a real workspace, not a placeholder window.

Deliverables:

- title bar
- collapsible navigation rail
- multi-tab workspace host
- status area
- placeholder routes for future studios
- docking behavior consistent with the design system

Requirements:

- Navigation rail on the left.
- Workspace in the center.
- AI panel reserved as a collapsible right-side area.
- Tabs must be capable of hosting future module views.
- The shell must be usable on large monitors and multi-monitor setups.

### 3. Theme System

Implement the app-wide theme architecture before feature work starts.

Deliverables:

- dark theme as default
- light theme option
- theme resource dictionaries
- color tokens matching the design system
- runtime theme switching

Requirements:

- No hardcoded colors in views when a resource can be used.
- Theme selection must persist across restarts.
- Styling must support Fluent UI WPF conventions.

### 4. Branding and Visual Identity

Apply the Planova identity system to the shell before expanding business
features.

Deliverables:

- logo placement in the header and app chrome
- navigation rail icons and selected-state styling
- branded header, spacing, and panel treatment
- dashboard cards and quick-action styling aligned to the brand
- AI assistant panel framing and hierarchy
- iconography pass for shell actions and module placeholders
- light and dark theme fidelity matching the reference brand artwork

Requirements:

- The shell must visibly look like Planova, not a generic WPF starter app.
- Logo, icon, spacing, and surface treatment must be consistent across the
  shell.
- Brand styling must be applied before any later studio reuses the shell.

### 5. Localization and RTL

Implement localization as a platform capability, not a later polish item.

Deliverables:

- English resources
- Arabic resources
- culture selection service
- runtime language switching
- RTL switching support
- localized shell labels and common commands

Requirements:

- No user-facing strings may remain hardcoded in the shell.
- Arabic layout must render correctly in right-to-left mode.
- Resource lookup must be centralized so later modules can reuse it.

### 6. Logging and Diagnostics

Add logging early so bootstrap and foundation issues are easy to trace.

Deliverables:

- Serilog bootstrapping
- console/file/app log sinks as appropriate for desktop development
- startup exception handling
- structured logs for shell, settings, and persistence events

Requirements:

- Logging must start before the UI host fully initializes.
- Application failures must be captured with useful context.

### 7. Settings and Externalized Configuration

Implement a settings model that is durable, testable, and easy to extend.

Deliverables:

- application settings abstraction
- theme and language preferences
- window behavior preferences
- future-safe storage model
- configuration binding for environment-specific options

Requirements:

- No secrets in source control.
- Settings must be persisted outside UI code.
- The storage format must support future expansion without breaking users.

### 8. Persistence Foundation

Prepare the data layer so later modules can start using it without redesign.

Deliverables:

- SQLite database bootstrap
- EF Core setup
- initial schema foundation
- migration pipeline
- connection factory or equivalent infrastructure service

Requirements:

- Code-first migrations.
- Strong entity modeling.
- Versioned schema.
- Audit support where it is appropriate from day one.

Notes:

- This phase does not need the full business schema.
- It does need the foundation to be stable enough for later module tables and
  relationships.

### 9. Validation and Smoke Tests

Before the phase is considered complete, verify the app behaves like a real
desktop product.

Deliverables:

- startup smoke test
- shell launch verification
- language switch verification
- theme switch verification
- settings persistence verification
- database initialization verification

Requirements:

- The application must start without manual setup beyond the documented
  installation steps.
- The shell must display at least one working navigation target.
- The app must survive a restart with settings preserved.

## Technical Decisions

### Application Host

Use `Microsoft.Extensions.Hosting` to wire up the desktop application lifecycle,
dependency injection, logging, and configuration.

### UI Framework

Use WPF with Fluent UI WPF and CommunityToolkit.Mvvm.

### Persistence

Use SQLite with EF Core as the initial persistence stack.

### Localization

Use resource-based localization with runtime culture switching and RTL-aware
layout behavior.

### Logging

Use Serilog for structured diagnostics and startup troubleshooting.

## Detailed Work Breakdown

### Workstream A: Foundation Projects

1. Create or verify the core project set.
2. Add project references according to Clean Architecture.
3. Add shared abstractions for application services, shell contracts, and
   localization interfaces.
4. Add build settings, nullable context, implicit usings, and analyzers.

### Workstream B: Shell and Navigation

1. Build the main window and shell view model.
2. Implement navigation rail commands and selection state.
3. Add a tab host that can open module placeholders.
4. Add a persistent status bar and command surface.
5. Add placeholder destinations for future modules without building them yet.

### Workstream C: Theme and Visual System

1. Create theme resource dictionaries.
2. Establish shared spacing, typography, and color tokens.
3. Add dark theme as the initial startup theme.
4. Implement light theme switching.
5. Persist selected theme in settings.

### Workstream D: Localization

1. Create the localization service and resource lookup abstraction.
2. Add English and Arabic resource files for common shell text.
3. Implement culture switching in the UI.
4. Verify RTL flow for Arabic.
5. Make localized strings available to future modules.

### Workstream E: Logging, Settings, and Boot

1. Configure startup logging before the shell loads.
2. Add application settings persistence.
3. Store theme, language, and basic window state preferences.
4. Add exception handling for startup and unhandled UI errors.

### Workstream F: Persistence

1. Create the SQLite database startup path.
2. Wire EF Core and register the persistence services.
3. Add the initial migration and versioning strategy.
4. Validate the app can create or open the database reliably.

### Workstream G: Validation

1. Launch the app on a clean environment.
2. Confirm shell navigation works.
3. Confirm theme switching works.
4. Confirm English and Arabic switching works.
5. Confirm settings survive restart.
6. Confirm the database initializes and logs expected events.

## Risks and Mitigations

### Risk: Foundation projects become too coupled too early

Mitigation:

- Keep shared abstractions thin.
- Put only platform-neutral contracts in shared layers.
- Push implementation details into Infrastructure or Persistence.

### Risk: Localization gets postponed and becomes expensive to retrofit

Mitigation:

- Make localization part of the shell, not a later enhancement.
- Ensure every shell string is resource-backed from the start.

### Risk: Theme and layout decisions leak hardcoded values into the UI

Mitigation:

- Centralize theme tokens.
- Use resource dictionaries and bindings instead of inline values.

### Risk: Persistence layer grows schema too early

Mitigation:

- Create only the foundation schema and migration path in Phase 0.
- Defer feature entities until the relevant studio phases.

## Acceptance Criteria

Phase 0 is complete when all of the following are true:

- The application launches to a working shell window.
- The shell uses Navigation Rail + Multi-Tab Workspace.
- The shell reflects the Planova brand identity, including logo, icons, and
  branded shell treatment.
- Dark theme is the default and light theme can be selected.
- English and Arabic resources load correctly.
- Runtime language switching works.
- RTL rendering works in Arabic mode.
- Settings persist across restart.
- Logging captures startup and runtime diagnostics.
- SQLite and EF Core are wired and can initialize successfully.
- The solution respects Clean Architecture and MVVM boundaries.
- No Phase 0 work introduces a workflow engine, automation designer, or AI
  provider coupling.

## Definition of Done

- Code is organized in the correct architecture layers.
- The shell is functional, not just visual.
- Localization is wired into the runtime.
- Theme and settings changes persist.
- Persistence starts cleanly.
- The implementation is documented enough for the next phase to build on it.

## Next Step After Phase 0

When the foundation is stable, the next implementation plan should target the
Phase 1 project management layer:

- Projects
- Clients
- Contracts
- User Profiles

Those capabilities should build directly on the shell, localization, logging,
and persistence foundation established here.
