# Tasks: Branding Visual Identity

**Input**: Design documents from `/specs/003-branding-visual-identity/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Not explicitly requested in the feature specification. Test tasks are omitted unless a visual verification task is noted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project root**: `Planova.UI/` for all UI source, `Planova.Shared/` for shared abstractions
- **Styles**: `Planova.UI/Styles/`
- **Views**: `Planova.UI/Views/`
- **ViewModels**: `Planova.UI/ViewModels/`
- **Converters**: `Planova.UI/Converters/`
- **Services**: `Planova.UI/Services/`
- **Resources**: `Planova.UI/Resources/Branding/`
- **Tests**: `tests/Planova.UI.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Package replacement, font bundling, asset preparation, and new XAML resource file creation. This phase must complete before any user story work begins.

- [X] T001 Remove FluentWPF 0.10.2 package from `Planova.UI/Planova.UI.csproj` and add Wpf.Ui package
- [X] T002 [P] Add Inter Variable font files (`Inter-Variable.ttf`, `Inter-Italic-Variable.ttf`) to `Planova.UI/Resources/Branding/Inter/` and register as `Resource` build action in `Planova.UI/Planova.UI.csproj`
- [X] T003 [P] Copy `LogoMonochrome.png` from `Planova Branding Master/Logo-Monochrome.png` to `Planova.UI/Resources/Branding/LogoMonochrome.png` and register as `Resource` build action
- [X] T004 Create `Planova.UI/Styles/HighContrastFallback.xaml` — resource dictionary that maps all brand token keys to `SystemColors` equivalents (WindowColorBrush, ControlTextBrush, etc.)
- [X] T005 Create `Planova.UI/Styles/Gradients.xaml` — resource dictionary with `ShellBackgroundGradient`, `DashboardBackgroundGradient`, and `WorkspaceBackgroundGradient` as `LinearGradientBrush` resources with dark and light theme-compatible gradient stops (high-contrast fallback to `SystemColors.WindowBrush`)
- [X] T006 Update `Planova.UI/App.xaml` merge order to: ThemeTokens → BrandStyles → BrandSpacing → Gradients → BrandColors → DarkTheme (or LightTheme/HighContrastFallback dynamically)
- [X] T007 Update `Planova.UI/Styles/ThemeTokens.xaml` — replace current default token values with authoritative palette values (`ThemeBackground`=`#0D1117`, `ThemeSurface`=`#161B22`, `ThemeCard`=`#1F2937`, `ThemeBorder`=`#2A3441`, `ThemeAccent`=`#00BFFF`, `ThemeSecondaryAccent`=`#0078D4`) and add `BrandFontFamily` token as `Segoe UI Variable, pack://application:,,,/Resources/Branding/Inter/#Inter`
- [X] T008 Update `Planova.UI/Styles/BrandColors.xaml` — align all color tokens to authoritative palette (extended brand palette: `BrandElevationLow`, `BrandElevationMedium`, `BrandHeaderForeground`, `BrandTextSecondary`, etc.) with dark-value and light-value overrides
- [X] T009 Update `Planova.UI/Styles/DarkTheme.xaml` — set all `ThemeToken` overrides to authoritative dark palette values (`ThemeBackground`=`#0D1117`, `ThemeSurface`=`#161B22`, `ThemeCard`=`#1F2937`, `ThemeBorder`=`#2A3441`, `ThemeAccent`=`#00BFFF`, `ThemeSecondaryAccent`=`#0078D4`)
- [X] T010 Update `Planova.UI/Styles/LightTheme.xaml` — set all `ThemeToken` overrides to authoritative light palette values (`ThemeBackground`=`#F8F9FB`, `ThemeSurface`=`#FFFFFF`, `ThemeCard`=`#FFFFFF`, `ThemeBorder`=`#D6DCE5`, `ThemeAccent`=`#0078D4`, `ThemeSecondaryAccent`=`#00BFFF`)
- [X] T011 Add `AppTheme` enum (`Dark`, `Light`, `HighContrast`) to `Planova.Shared/Abstractions/` and update `IThemeService` interface to include `AppTheme CurrentTheme`, `void SetTheme(AppTheme theme)`, and `event EventHandler<ThemeChangedEventArgs> ThemeChanged`
- [X] T012 Create `Planova.UI/Services/HighContrastDetector.cs` implementing `IHighContrastDetector` with `IsHighContrast` property and `HighContrastChanged` event, listening to `SystemParameters.StaticPropertyChanged` for `HighContrast` property changes, and registering with DI container in `App.xaml.cs`
- [X] T013 Update `Planova.UI/Services/ThemeService.cs` to implement enhanced `IThemeService` — support `AppTheme.HighContrast` which removes `BrandColors.xaml` and `Gradients.xaml` from merged dictionaries and applies `HighContrastFallback.xaml`; support `AppTheme.Dark`/`AppTheme.Light` with proper dictionary swap including `Gradients.xaml`; integrate with `HighContrastDetector` so OS high-contrast toggle automatically triggers `SetTheme(AppTheme.HighContrast)` or restores previous theme
- [X] T014 Create `Planova.UI.Tests` project (xUnit) with reference to `Planova.UI`, and add initial `TokenConsistencyTests.cs` verifying that all authoritative palette hex values resolve correctly in theme dictionaries

**Checkpoint**: All theme tokens, gradients, high-contrast fallback, Wpf.Ui package, and Inter font are in place. Foundation ready for user story work.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core service and infrastructure changes that MUST be complete before ANY user story can be implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T015 Update `NavigationTargetInfo` record in `Planova.Shared/Abstractions/INavigationService.cs` to add `IconGlyph` (string), `IsStudio` (bool), and `IsPlaceholder` (bool) fields
- [X] T016 Update `NavigationItemViewModel` in `Planova.UI/ViewModels/NavigationItemViewModel.cs` to include `Icon` property of type `Wpf.Ui.Controls.SymbolRegular` and `IsPlaceholder` property, mapping from `NavigationTargetInfo.IconGlyph`
- [X] T017 Update `ShellViewModel.RegisterNavigationTargets()` in `Planova.UI/ViewModels/ShellViewModel.cs` to register all 17 navigation targets with Fluent UI icon glyph names, `IsStudio` flags, and `IsPlaceholder` flags per the data-model navigation item table (dashboard, projects, boq, wbs, activity, resource, cost, reports, primavera, schedule-compare, delay-analysis, claims, chronology, correspondence, knowledge-base, analytics, integration-hub, settings)
- [X] T018 Update `Planova.UI/Converters/NavIconConverter.cs` to map navigation item IDs to `Wpf.Ui.SymbolRegular` enum values (replacing Segoe MDL2 glyph codes); or replace converter usage entirely with direct `SymbolIcon` binding from `NavigationItemViewModel.Icon`
- [X] T019 Update `Planova.UI/Views/NavigationRail.xaml` to render icons using `Wpf.Ui.Controls.SymbolIcon` bound to `NavigationItemViewModel.Icon` instead of Segoe MDL2 font path, with branded hover/selected/pressed/focus/disabled visual states applied via style triggers referencing `ThemeAccent`, `ThemeSurface`, `ThemeBorder` tokens
- [X] T020 Update `Planova.UI/Views/ShellView.xaml` to integrate `Wpf.Ui.TitleBar` control in the header row, placing the Planova logo `Image` in the `TitleBar.Icon` slot and setting `TitleBar.Header` to include the wordmark; verify the existing collapse behavior (wordmark hides below 640px) still functions with `TitleBar`
- [X] T021 Update `Planova.UI/Styles/BrandStyles.xaml` — apply `BrandFontFamily` to all named text styles (`DashboardTitleStyle`, `SectionHeaderStyle`, `BodyTextStyle`, `KpiValueStyle`, `KpiLabelStyle`, `EmptyStateTitleStyle`, `EmptyStateDescriptionStyle`); update `CardSurfaceStyle`, `KpiCardStyle`, `PanelBorderStyle`, `WorkspaceTabStyle`, `ActionTileStyle` to reference updated color tokens (`ThemeCard`, `ThemeBorder`, `ThemeAccent`, `BrandElevationLow`, etc.); add `AIAssistantCollapsedStyle` for 48px collapsed indicator strip; update `AIAssistantPanelStyle` for 320px expanded panel with branded surface and chrome styling
- [X] T022 Update `Planova.UI/ViewModels/ShellViewModel.cs` — integrate `IThemeService.SetTheme(AppTheme)` calls for dark/light/high-contrast; replace manual `ResourceDictionary` swap logic with `ThemeService` calls; add `HighContrastDetector` subscription to auto-switch to `AppTheme.HighContrast` when OS high-contrast changes; preserve theme preference persistence via `ISettingsService`

**Checkpoint**: Foundation ready — 17 nav items registered, Wpf.Ui integrated, theme service refactored, high-contrast detection active, brand styles updated. User story implementation can now begin.

---

## Phase 3: User Story 1 - Experience Branded Application Shell (Priority: P1) 🎯 MVP

**Goal**: A user launching the application immediately sees the Planova brand identity through logo placement, branded navigation rail, and consistent visual language.

**Independent Test**: Launch the application and visually verify: logo and wordmark in top-left header area, all 17 navigation items have branded Fluent UI icons with consistent style, spacing/surfaces/composition match brand reference assets.

### Implementation for User Story 1

- [X] T023 [P] [US1] Update `Planova.UI/Views/ShellView.xaml` — apply `Background="{DynamicResource ShellBackgroundGradient}"` to the shell root element; apply `ThemeSurface` background to the navigation rail column; apply `BrandFontFamily` to all text elements not yet covered; ensure header row uses `BrandElevationLow` background and `BrandHeaderForeground` foreground
- [X] T024 [P] [US1] Update `Planova.UI/Converters/ThemeAwareLogoConverter.cs` — add support for `AppTheme.HighContrast` returning `LogoMonochrome.png`; ensure Dark → `LogoDark.png`, Light → `LogoLight.png` mapping still works; verify pack URI paths reference `Resources/Branding/`
- [X] T025 [US1] Update `Planova.UI/Views/ShellView.xaml` title bar area — ensure `Wpf.Ui.TitleBar` renders logo in `Icon` slot, wordmark in `Header` slot, and tabs sit integrated in the same row as the title bar per the implementation plan's "Top Bar Composition" requirement
- [X] T026 [US1] Update `Planova.UI/Views/NavigationRail.xaml` — verify all 17 items render with Fluent UI `SymbolIcon`, display localized labels, show branded selected/hover/pressed/focus/disabled states per the VisualState spec in data-model; apply `BrandSpacing` tokens for item padding; ensure collapsible behavior (220px expanded / 48px collapsed)
- [X] T027 [P] [US1] Create branded empty-state view template in `Planova.UI/Views/EmptyStateView.xaml(.cs)` — a `UserControl` using `EmptyStateContainerStyle`, `EmptyStateIconStyle` (with Fluent UI `SymbolIcon`), `EmptyStateTitleStyle`, and `EmptyStateDescriptionStyle`; bind icon/title/description to ViewModel properties for placeholder navigation targets
- [X] T028 [US1] Update `Planova.UI/ViewModels/ShellViewModel.cs` — for each placeholder navigation target (IsPlaceholder=true), register a view factory that returns `EmptyStateView` with appropriate icon and localized title/description; for existing functional targets (dashboard, projects, clients, contracts, profile, reports, settings), retain current view factories
- [X] T029 [US1] Update `Planova.UI/Views/ShellView.xaml` — ensure the status bar area uses branded styling (`ThemeSurface` background, `ThemeBorder` top border, `BrandTextSecondary` foreground); verify theme toggle button and language toggle button use `ThemeAccent` styling
- [X] T030 [P] [US1] Add wordmark collapse behavior — verify the existing `ShellView.xaml.cs` responsive width handler (wordmark hides below 640px) still works with `Wpf.Ui.TitleBar` integration; update `ThemeAwareLogoConverter` to ensure graceful fallback to logo-only mode; add visual state transitions for wordmark visibility change
- [ ] T031 [US1] Visual verification — launch the application, verify all 17 navigation items appear with Fluent UI icons, logo appears in title bar, wordmark appears in header row, shell background uses adaptive gradient, dark theme colors match authoritative palette, spacing matches brand reference, wordmark collapses below 640px

**Checkpoint**: At this point, User Story 1 should be fully functional. The branded shell is visible at launch with logo, wordmark, 17 icon-based nav items, adaptive gradient background, and consistent visual language.

---

## Phase 4: User Story 2 - Experience Branded Dashboard (Priority: P1)

**Goal**: A user viewing the dashboard sees branded KPI cards, quick action tiles, and section hierarchy that reflects the Planova product identity.

**Independent Test**: Open the dashboard and verify KPI cards, action buttons, and section headers use branded styling consistent with the reference assets.

### Implementation for User Story 2

- [X] T032 [US2] Update `Planova.UI/Views/Dashboard/DashboardView.xaml` — apply `Background="{DynamicResource DashboardBackgroundGradient}"` to the dashboard root; update all `KpiCardStyle` references to use authoritative token values (`ThemeCard` for surface, `ThemeAccent` for value highlighting, `ThemeBorder` for card borders); apply `BrandFontFamily` to `KpiValueStyle` and `KpiLabelStyle`; verify card dimensions (200x120) and `BrandSpacing` padding
- [X] T033 [US2] Update `Planova.UI/Views/Dashboard/DashboardView.xaml` — update quick action tiles to use `ActionTileStyle` with Fluent UI `SymbolIcon` icons (replacing any bitmap or non-Fluent icons), branded surface tokens, consistent tile size, and `BrandSpacingMedium` gaps between tiles
- [X] T034 [US2] Update `Planova.UI/Views/Dashboard/DashboardView.xaml` — apply `DashboardTitleStyle` for section headers, ensure `SectionHeaderBaseStyle` inherits `BrandFontFamily` and uses `ThemeAccent` or `BrandElevationLow` for hierarchy; verify spacing follows `BrandSpacing` tokens; ensure recent activity list uses `BodyTextStyle` with themed surface colors
- [X] T035 [US2] Update `Planova.UI/Styles/BrandStyles.xaml` — refine `KpiCardStyle` to include branded hover state (subtle `ThemeAccent` border on mouse enter), ensuring the card style uses authoritative palette values and `BrandRadiusMedium` for corner radius; update `ActionTileStyle` to include hover/pressed states with accent feedback
- [ ] T036 [US2] Visual verification — open the dashboard, verify KPI cards with branded surfaces, quick actions with Fluent UI icons, section headers with hierarchy, adaptive gradient background, spacing matches brand reference within 90% visual fidelity

**Checkpoint**: Dashboard is now fully branded with KPI cards, quick actions, and section hierarchy matching the brand reference layout.

---

## Phase 5: User Story 3 - Switch Between Dark and Light Branded Themes (Priority: P2)

**Goal**: A user switching between dark and light themes sees both modes render with the same brand identity, not just minimum accessible contrast. High-contrast mode fully suppresses brand colors.

**Independent Test**: Switch between dark and light themes and compare both modes against brand reference assets for logo visibility, surface colors, and brand cohesion. Toggle Windows high-contrast mode and verify all brand colors are suppressed.

### Implementation for User Story 3

- [X] T037 [US3] Verify `Planova.UI/Services/ThemeService.cs` — confirm dark→light switch correctly swaps `DarkTheme.xaml` for `LightTheme.xaml` in merged dictionaries while preserving `BrandColors.xaml`, `Gradients.xaml`, and `BrandStyles.xaml`; confirm `ThemeChanged` event fires after dictionary swap; verify `ThemeAwareLogoConverter` updates logo variant (Dark theme → `LogoDark.png`, Light theme → `LogoLight.png`)
- [X] T038 [US3] Verify adaptive gradients — confirm `ShellBackgroundGradient`, `DashboardBackgroundGradient`, and `WorkspaceBackgroundGradient` correctly switch between dark and light gradient stops when theme changes; confirm gradient brushes resolve via `DynamicResource` (not `StaticResource`) so theme switches propagate immediately
- [X] T039 [US3] Implement high-contrast fallback — verify `HighContrastDetector` correctly detects `SystemParameters.HighContrast` changes; when high-contrast activates, confirm `ThemeService.SetTheme(AppTheme.HighContrast)` removes `BrandColors.xaml` and `Gradients.xaml` from merged dictionaries, adds `HighContrastFallback.xaml`, and all surfaces render with `SystemColors.WindowBrush`, `SystemColors.WindowTextBrush`, etc.; when high-contrast deactivates, confirm previous theme (Dark/Light) is restored with brand dictionaries
- [X] T040 [US3] Add `ThemeChangedEventArgs` class to `Planova.Shared/Abstractions/` carrying `AppTheme NewTheme` property; verify `ShellViewModel` subscribes to `IThemeService.ThemeChanged` and updates logo/gradient/icon state on theme change
- [X] T041 [P] [US3] Verify tab styling across themes — confirm `WorkspaceTabStyle` in `BrandStyles.xaml` renders active tab with `ThemeAccent` underline, inactive tabs with `ThemeBorder`/`ThemeSurface` treatment, and both states look cohesive in Dark and Light modes; apply `BrandFontFamily` to tab text
- [ ] T042 [US3] Visual verification — switch between dark and light themes, verify logo variants update, gradient backgrounds shift, surface colors/borders/elevations feel like same brand family, KPI cards and panels maintain consistent visual hierarchy across modes; toggle OS high-contrast and verify all brand colors are suppressed with system colors replacing them

**Checkpoint**: Theme switching is fully functional with brand-cohesive appearance in dark, light, and high-contrast modes.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: AI assistant panel, RTL support, edge cases, and final verification.

- [X] T043 Create `Planova.UI/Views/AI/AssistantPanelView.xaml(.cs)` — a `UserControl` hosting a collapsible panel (48px collapsed indicator `Border` with Fluent UI `Bot` icon → 320px expanded panel with branded surface, `AIAssistantPanelStyle`); collapsed state shows accent-colored icon strip matching nav rail rhythm; expanded state shows assistant content area with branded header, input area, and `ThemeSurface` background; bind expand/collapse toggle to ViewModel command
- [X] T044 Create `Planova.UI/ViewModels/AssistantPanelViewModel.cs` — `ObservableObject` with `IsExpanded` property (default `false`), `ToggleExpandCommand`, `Title` and `StatusText` properties; register in DI container
- [X] T045 Integrate `AssistantPanelView` into `Planova.UI/Views/ShellView.xaml` — add to body grid as right-side column (auto-width collapsed, 320px expanded); wire `IsExpanded` binding; the panel should be persistent across module navigation (not a navigation target)
- [X] T046 Update `Planova.UI/Views/WorkspaceArea.xaml` — apply `Background="{DynamicResource WorkspaceBackgroundGradient}"` to workspace root; ensure tab control uses `WorkspaceTabStyle` from `BrandStyles.xaml`; verify inactive tab treatment uses `ThemeSurface`/`ThemeBorder` tokens
- [X] T047 [P] Update all workspace views (`ProjectsWorkspaceView.xaml`, `ClientsWorkspaceView.xaml`, `ContractsWorkspaceView.xaml`, `UserProfileView.xaml`, `ReportView.xaml`) — apply `WorkspaceBackgroundGradient` background; ensure any hardcoded colors are replaced with `DynamicResource` theme token references; apply `BrandFontFamily` to all text elements
- [ ] T048 RTL support — verify `NavigationRail.xaml` supports `FlowDirection="RightToLeft"` when Arabic locale is active; ensure logo and wordmark remain left-anchored (per clarification: logo stays left, navigation chrome mirrors); verify `ShellView.xaml` respects `FrameworkElement.FlowDirection` based on `ILocalizationService.CurrentLanguage`
- [X] T049 Missing asset fallback — update `ThemeAwareLogoConverter.cs` to catch `FileNotFoundException` and return `LogoMonochrome.png` as fallback with a `Serilog` warning log; update all `SymbolIcon` usages to specify `FallbackIconSource` or show a generic placeholder `SymbolRegular.Info` when the specified icon glyph fails to render
- [X] T050 Minimum size thresholds — update `ShellView.xaml.cs` to implement `MinWidth`/`MinHeight` on shell window (800x600 minimum); ensure `NavigationRail` collapses to icon-only below 900px width; ensure wordmark hides below 640px (existing behavior); logo-only mode in title bar when wordmark is hidden
- [X] T051 Remove unused `Planova.UI/MainWindow.xaml(.cs)` — this stub `Window` is never referenced from `App.xaml.cs`; remove it to eliminate dead code
- [ ] T052 Final visual verification — run through all acceptance scenarios from spec.md: launch app (logo + wordmark visible), all 17 nav items with branded icons, dashboard KPI cards with branded surfaces, theme switch (dark/light/high-contrast), AI assistant panel collapsed/expanded, workspace tabs with branded styling, adaptive gradients on all surfaces, wordmark collapse, RTL layout, missing asset fallback, WCAG AA contrast verification with automated tool

**Checkpoint**: All polish items complete. Full branding pass is ready for final review against brand reference assets.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational. Can start after Phase 2.
- **User Story 2 (Phase 4)**: Depends on Foundational. Can start after Phase 2. Shares shell surface tokens with US1 (parallel-safe but integrated testing benefits from US1 completion).
- **User Story 3 (Phase 5)**: Depends on Foundational. Can start after Phase 2. Best tested after US1 and US2 are visually complete.
- **Polish (Phase 6)**: Depends on US1, US2, and US3 completion.

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational only. No dependency on other stories.
- **User Story 2 (P2)**: Depends on Foundational only. Dashboard styling can proceed independently, but benefits from US1's shell gradient and nav rail being in place.
- **User Story 3 (P2)**: Depends on Foundational only. Theme switching touches all surfaces, so best tested after US1+US2 are in place.

### Within Each User Story

- XAML resource updates (styles, tokens) before View updates
- ViewModel changes before View bindings
- Visual verification after all implementation tasks

### Parallel Opportunities

- Phase 1: T002, T003 can run in parallel (different resource files)
- Phase 2: T015-T018 can partially overlap (different files, shared interface)
- Phase 3 (US1): T023, T024, T027, T030 can run in parallel
- Phase 4 (US2): T032, T033, T034 can start once T035 is done (shared BrandStyles.xaml)
- Phase 6: T043-T044 can run in parallel; T047 can run in parallel across views

---

## Parallel Example: User Story 1

```text
# These tasks touch different files and can run simultaneously:
Task T023: "Apply ShellBackgroundGradient and token updates in ShellView.xaml"
Task T024: "Update ThemeAwareLogoConverter for HighContrast monochrome fallback"
Task T027: "Create EmptyStateView.xaml(.cs) template"
Task T030: "Verify wordmark collapse behavior with TitleBar integration"

# These have sequential dependencies within US1:
T025 → T026  (TitleBar integration before nav rail verification)
T028 → T027  (ShellViewModel needs EmptyStateView to register)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T014)
2. Complete Phase 2: Foundational (T015–T022)
3. Complete Phase 3: User Story 1 (T023–T031)
4. **STOP and VALIDATE**: Launch app, verify branded shell with logo, wordmark, 17 Fluent UI icons, adaptive gradient, dark theme
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add User Story 1 → Test branded shell independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test branded dashboard independently → Deploy/Demo
4. Add User Story 3 → Test theme switching + high-contrast → Deploy/Demo
5. Add Polish (assistant panel, RTL, edge cases) → Final validation → Deploy

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Shell + Navigation + Logo)
   - Developer B: User Story 2 (Dashboard Cards + Actions)
   - Developer C: User Story 3 (Theme service + High-contrast)
3. Polish after all stories integrate

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- The `MainWindow.xaml` stub (T051) should be removed only after confirming `ShellView` is the sole application window
- Wpf.Ui `SymbolRegular` enum values must be verified at implementation time against the specific Wpf.Ui version installed