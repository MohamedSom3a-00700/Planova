# Implementation Plan: Branding Visual Identity

**Branch**: `003-branding-visual-identity` | **Date**: 2026-06-01 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-branding-visual-identity/spec.md`

## Summary

Apply the Planova brand system to the application shell, navigation rail, dashboard, workspace chrome, and AI assistant panel. This pass modifies visual properties only — no new business features, domain logic, or database changes. The work involves: updating theme color tokens to match the authoritative design system palette, replacing icon rendering with Fluent UI icon glyphs for all 17 navigation items, adding a bundled Inter font fallback, implementing adaptive gradient backgrounds, applying branded styling to dashboard cards/tabs/assistant panel/empty states, adding window title bar branding, and ensuring dark/light theme fidelity plus high-contrast mode fallback.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: WPF, FluentWPF 0.10.2 (to be evaluated for replacement with Wpf.Ui), CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.Hosting 8.0.1, Serilog

**Storage**: SQLite via EF Core (no schema changes in this pass)

**Testing**: xUnit (Application.Tests, Domain.Tests, Persistence.Tests), manual visual verification against brand reference assets

**Target Platform**: Windows 10+ desktop (WPF)

**Project Type**: desktop-app

**Performance Goals**: Theme switch <100ms, shell render <200ms first frame, no perceptible lag on navigation item hover/state changes

**Constraints**: WCAG AA contrast ratios, desktop resolutions only, no mobile/responsive, high-contrast mode must fully suppress brand colors

**Scale/Scope**: 17 navigation items, ~10 Views, 6 XAML resource dictionaries, 4 logo variants to embed, 1 font to bundle

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Architecture First | PASS | Brancing pass only modifies UI layer. No business logic in Views. MVVM preserved. |
| II. MVVM & Fluent UI Enforcement | PASS | MVVM pattern maintained. FluentWPF 0.10.2 evaluates for replacement with Wpf.Ui to enable proper Fluent UI icon support. |
| III. Modular Domain Design | PASS | No new domain modules. Visual tokens are UI-layer concerns only. |
| IV. Build vs Buy Strategy | PASS | Using Fluent UI icon system (buy/adopt). Bundling Inter font (adopt). No bespoke rendering engine. |
| V. Automation Platform Agnostic | N/A | No automation changes in this pass. |
| VI. AI Provider Agnostic | N/A | No AI provider changes. Assistant panel is purely visual chrome. |
| VII. Multilingual First | PASS | RTL support for navigation mirroring already specified. Logo stays left-anchored per clarification. Existing localization service preserved. |
| VIII. Performance & Scalability | PASS | Theme token swaps are synchronous resource dictionary merges (already async event-driven). Font bundling is static resource. No thread blocking introduced. |

**Gate Result**: PASS — No violations. All 4 applicable principles satisfied.

## Project Structure

### Documentation (this feature)

```text
specs/003-branding-visual-identity/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Planova.UI/
├── App.xaml                        # Merge order: ThemeTokens → BrandStyles → Dark/LightTheme
├── App.xaml.cs                     # DI container, startup, host builder
├── MainWindow.xaml(.cs)            # Unused — consider removing or repurposing
├── Styles/
│   ├── ThemeTokens.xaml            # Base/default theme token definitions
│   ├── BrandColors.xaml            # Brand color palette (authoritative hex values)
│   ├── BrandSpacing.xaml           # Spacing, radius, font-size tokens
│   ├── BrandStyles.xaml            # Named styles (cards, panels, tabs, empty states)
│   ├── DarkTheme.xaml              # Dark theme overrides (merges BrandColors + BrandSpacing)
│   ├── LightTheme.xaml             # Light theme overrides (merges BrandColors + BrandSpacing)
│   ├── HighContrastFallback.xaml   # NEW: OS high-contrast override dictionary
│   └── Gradients.xaml              # NEW: Adaptive gradient brush definitions
├── Views/
│   ├── ShellView.xaml(.cs)         # Main shell window (header, nav rail, workspace, status)
│   ├── NavigationRail.xaml(.cs)   # Collapsible left nav (220px/48px)
│   ├── WorkspaceArea.xaml(.cs)     # Tab-based workspace container
│   ├── Dashboard/
│   │   └── DashboardView.xaml(.cs) # KPI cards, quick actions, recent activity
│   ├── Projects/
│   │   └── ProjectsWorkspaceView.xaml(.cs)
│   ├── Clients/
│   │   └── ClientsWorkspaceView.xaml(.cs)
│   ├── Contracts/
│   │   └── ContractsWorkspaceView.xaml(.cs)
│   ├── Profile/
│   │   └── UserProfileView.xaml(.cs)
│   ├── Reports/
│   │   └── ReportView.xaml(.cs)
│   └── AI/
│       └── AssistantPanelView.xaml(.cs)  # NEW: Branded AI assistant panel
├── ViewModels/
│   ├── ShellViewModel.cs           # Navigation targets, theme toggle, tab management
│   ├── NavigationItemViewModel.cs   # Nav item DTO
│   ├── WorkspaceTabViewModel.cs    # Tab DTO
│   └── DashboardViewModel.cs      # KPI data loading
├── Converters/
│   ├── ThemeAwareLogoConverter.cs  # Theme-aware logo selection (exists)
│   └── NavIconConverter.cs         # Nav icon glyph (currently Segoe MDL2 — needs update)
├── Resources/
│   └── Branding/
│       ├── LogoDark.png            # Dark theme logo (exists)
│       ├── LogoLight.png           # Light theme logo (exists)
│       ├── LogoMonochrome.png      # NEW: Monochrome variant
│       ├── Wordmark.png            # Wordmark (exists)
│       └── Inter/
│           ├── Inter-Variable.ttf  # NEW: Bundled Inter font
│           └── Inter-Italic-Variable.ttf  # NEW: Bundled Inter italic
└── Services/
    ├── ThemeService.cs             # IThemeService implementation (exists)
    └── HighContrastDetector.cs     # NEW: OS high-contrast detection

Planova.Domain/
├── Entities/                       # No changes
└── ValueObjects/                   # No changes

Planova.Shared/
└── Abstractions/
    └── INavigationService.cs       # Register 17 targets (currently 11)

Planova.Application/                # No changes
Planova.Infrastructure/             # No changes
Planova.Persistence/                 # No changes (no schema changes)
Planova.Localization/                # No changes (RTL already supported)

tests/
├── Planova.UI.Tests/               # NEW: Visual token and style verification tests
│   ├── TokenConsistencyTests.cs
│   └── HighContrastFallbackTests.cs
├── Application.Tests/
├── Domain.Tests/
└── Persistence.Tests/
```

**Structure Decision**: Existing Clean Architecture solution structure is preserved. Branding changes are confined to `Planova.UI` (Styles/, Views/, ViewModels/, Converters/, Resources/). A new `Planova.UI.Tests` project is added for visual token verification. No domain, application, infrastructure, or persistence changes are required.

## Complexity Tracking

> No constitution violations to justify. The gate passed cleanly.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none)    | —          | —                                    |