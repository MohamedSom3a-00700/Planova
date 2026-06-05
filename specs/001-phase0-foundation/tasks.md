---

description: "Task list for Planova Phase 0 Foundation - WPF desktop shell with navigation, themes, localization, settings, and database baseline"
---

# Tasks: Planova Phase 0 Foundation

**Input**: Design documents from `specs/001-phase0-foundation/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not requested in spec. No test tasks generated.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Solution root**: `Planova.sln` at repository root
- **Projects**: `Planova.UI/`, `Planova.Application/`, `Planova.Domain/`, `Planova.Infrastructure/`, `Planova.Persistence/`, `Planova.Localization/`, `Planova.Shared/`

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Create .NET solution, project structure, and NuGet package dependencies

- [ ] T001 Create Planova.sln with 7 projects per Clean Architecture plan: Planova.UI (WPF), Planova.Application (classlib), Planova.Domain (classlib), Planova.Infrastructure (classlib), Planova.Persistence (classlib), Planova.Localization (classlib), Planova.Shared (classlib)
- [ ] T002 [P] Add NuGet packages: Fluent.UI.WPF, CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting, Serilog, Serilog.Sinks.File, Serilog.Sinks.Console, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Tools
- [ ] T003 [P] Configure Serilog bootstrapping in Planova.UI/Program.cs with console sink during startup before host initialization
- [ ] T004 [P] Create initial App.xaml with merged resource dictionary structure pointing to Styles/ folder

---

## Phase 2: Foundational (Shared Infrastructure)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 [P] Create ILoggingService interface in Planova.Shared/Abstractions/ILoggingService.cs
- [ ] T006 Implement SerilogLoggingService in Planova.Infrastructure/Logging/SerilogLoggingService.cs wrapping Serilog with Info/Error/Warning methods
- [ ] T007 Configure Microsoft.Extensions.Hosting with DI service registration for all foundational services in Planova.UI/Program.cs
- [ ] T008 Set up project references respecting Clean Architecture layers: UI references Application.Shared, Application references Domain, Infrastructure/Persistence reference Application, Shared has no dependencies

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Application Launch and Shell (Priority: P1) MVP

**Goal**: Application starts with a working shell window containing navigation rail, multi-tab workspace, and status bar

**Independent Test**: Launch the application and verify the shell window appears within 5 seconds with a navigation rail on the left, multi-tab workspace in the center, and status bar at the bottom

### Implementation for User Story 1

- [ ] T009 [P] [US1] Create INavigationService interface in Planova.Shared/Abstractions/INavigationService.cs with NavigateTo, RegisterTarget, GetActiveTarget methods
- [ ] T010 [P] [US1] Create ShellView.xaml with navigation rail (left), workspace area (center), and status bar (bottom) layout in Planova.UI/Views/ShellView.xaml
- [ ] T011 [P] [US1] Create ShellViewModel.cs with ObservableObject base, navigation state, and tab collection in Planova.UI/ViewModels/ShellViewModel.cs
- [ ] T012 [US1] Implement NavigationService in Planova.Application/Services/NavigationService.cs with target registration and tab activation logic
- [ ] T013 [US1] Create NavigationRail custom control in Planova.UI/Views/NavigationRail.xaml with collapsible behavior and target buttons
- [ ] T014 [US1] Create WorkspaceArea with TabControl-based multi-tab support in Planova.UI/Views/WorkspaceArea.xaml
- [ ] T015 [US1] Register placeholder navigation targets (BOQ, WBS, Scheduling, Claims, Settings) with placeholder views in ShellViewModel
- [ ] T016 [US1] Wire ShellView as application main window, verify launch with no errors, and validate 5-second startup target

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Theme Switching (Priority: P1)

**Goal**: Users can switch between dark and light themes at runtime with all UI elements updating immediately

**Independent Test**: Launch the app, switch from dark to light theme, and verify all UI elements update immediately without visual artifacts (persistence across restarts requires US4)

### Implementation for User Story 2

- [ ] T017 [P] [US2] Create IThemeService interface in Planova.Shared/Abstractions/IThemeService.cs with SetTheme, GetCurrentTheme, ThemeChanged event
- [ ] T018 [P] [US2] Create DarkTheme.xaml resource dictionary in Planova.UI/Styles/ with all dark palette colors, brushes, and control templates
- [ ] T019 [P] [US2] Create LightTheme.xaml resource dictionary in Planova.UI/Styles/ with light palette colors, brushes, and control templates
- [ ] T020 [P] [US2] Create shared ThemeTokens.xaml with named brush keys (e.g., ThemeBackground, ThemeForeground, ThemeAccent) used by both themes in Planova.UI/Styles/
- [ ] T021 [US2] Implement ThemeService in Planova.Application/Services/ThemeService.cs that merges/unmerges theme resource dictionaries at runtime
- [ ] T022 [US2] Add theme toggle UI (e.g., toggle button in status bar or settings panel) bound to ThemeService in ShellView
- [ ] T023 [US2] Audit all ShellView and NavigationRail XAML to ensure zero hardcoded colors - all visual properties reference theme resource keys

**Checkpoint**: At this point, User Story 2 should be independently testable (runtime switching)

---

## Phase 5: User Story 3 - Language Switching (Priority: P1)

**Goal**: Users can switch between English and Arabic at runtime with correct RTL layout

**Independent Test**: Launch the app, switch language from English to Arabic, and verify all shell labels update and layout flows right-to-left (persistence across restarts requires US4)

### Implementation for User Story 3

- [ ] T024 [P] [US3] Create ILocalizationService interface in Planova.Shared/Abstractions/ILocalizationService.cs with SetLanguage, GetString, GetCurrentLanguage, IsRtl, LanguageChanged event
- [ ] T025 [P] [US3] Create English resource file Planova.Localization/Resources/Strings.en.resx with all shell strings (nav labels, status bar text, theme/language toggle labels, settings labels)
- [ ] T026 [P] [US3] Create Arabic resource file Planova.Localization/Resources/Strings.ar.resx with translated Arabic shell strings
- [ ] T027 [US3] Implement LocalizationService in Planova.Localization/Services/LocalizationService.cs that switches CurrentUICulture and raises LanguageChanged
- [ ] T028 [US3] Add language switcher UI (e.g., EN/AR toggle button in status bar) in ShellView bound to LocalizationService
- [ ] T029 [US3] Implement FlowDirection switching on ShellView root element bound to IsRtl property from LocalizationService
- [ ] T030 [US3] Ensure all XAML labels use x:Static or binding to LocalizationService.GetString - no hardcoded display strings in views

**Checkpoint**: At this point, User Story 3 should be independently testable (runtime switching)

---

## Phase 6: User Story 5 - Database Initialization (Priority: P2)

**Goal**: Local SQLite database initializes automatically on first launch with versioned schema management

**Independent Test**: Launch the app on a clean environment and verify the database file is created and schema initializes without errors

### Implementation for User Story 5

- [ ] T031 [P] [US5] Create IDatabaseService interface in Planova.Shared/Abstractions/IDatabaseService.cs with InitializeAsync, GetConnection, IsInitialized methods
- [ ] T032 [P] [US5] Create SchemaVersion entity in Planova.Domain/Entities/SchemaVersion.cs with Id, Version, AppliedAt, Description properties
- [ ] T033 [P] [US5] Create PlanovaDbContext in Planova.Persistence/DbContext/PlanovaDbContext.cs with DbSet for SchemaVersion and UserPreferences
- [ ] T034 [P] [US5] Configure PlanovaDbContext with SQLite provider and connection string pointing to %APPDATA%/Planova/planova.db in Planova.Persistence/DbContext/PlanovaDbContext.cs OnConfiguring
- [ ] T035 [US5] Implement DatabaseService in Planova.Persistence/DbContext/DatabaseService.cs with InitializeAsync creating DB and applying migrations
- [ ] T036 [US5] Create initial EF Core Code First migration for Phase 0 schema (UserPreferences + SchemaVersion tables) in Planova.Persistence/Migrations/
- [ ] T037 [US5] Wire DatabaseService initialization into application startup sequence in Planova.UI/Program.cs before shell loads
- [ ] T038 [US5] Add error handling for database init failure - show user-friendly dialog, log details, allow retry/exit

**Checkpoint**: Database infrastructure ready for settings persistence and future entity additions

---

## Phase 7: User Story 4 - Settings Persistence (Priority: P2)

**Goal**: User preferences (theme, language, window state) survive application restarts

**Independent Test**: Change theme and language, restart the app, and verify preferences are restored (requires US5 database)

### Implementation for User Story 4

- [ ] T039 [P] [US4] Create ISettingsService interface in Planova.Shared/Abstractions/ISettingsService.cs with Load, Save, Get\<T\>, Set\<T\> methods
- [ ] T040 [P] [US4] Create UserPreferences entity singleton mapping in Planova.Persistence/EntityConfigurations/UserPreferencesConfiguration.cs (single-row table pattern)
- [ ] T041 [US4] Implement SettingsService in Planova.Application/Services/SettingsService.cs reading/writing UserPreferences through PlanovaDbContext
- [ ] T042 [US4] Wire SettingsService.Load on startup to restore theme (ThemeService.SetTheme) and language (LocalizationService.SetLanguage) from persisted preferences
- [ ] T043 [US4] Implement window state save/restore: save X, Y, Width, Height, Maximized on close; restore on launch
- [ ] T044 [US4] Save preferences on each theme/language change and on application exit via SettingsService.Save

**Checkpoint**: Full persistence working across restarts

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Error handling, edge cases, and final validation

- [ ] T045 [P] Implement global unhandled exception handler in App.xaml.cs that logs full context via Serilog and shows user-friendly error dialog
- [ ] T046 [P] Add edge case handling: restricted file permissions (permission error dialog), corrupted database (reset option dialog), missing resource file (fallback to English with one-time notification)
- [ ] T047 [P] Configure Serilog rolling file sink with 7-day/100MB retention policy in Planova.UI/Program.cs
- [ ] T048 [P] Add display configuration change handling (DPI/scale changes) - layout adapts without crashing
- [ ] T049 Run quickstart.md validation: launch app, verify shell, test theme/language switch, verify DB creation, check log output

**Checkpoint**: Phase 0 feature complete and validated

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **US1 - Shell (Phase 3)**: Depends on Foundational - No dependencies on other stories
- **US2 - Theme (Phase 4)**: Depends on Foundational - Can start after US1 or in parallel; persistence requires US4
- **US3 - Language (Phase 5)**: Depends on Foundational - Can start after US1 or in parallel; persistence requires US4
- **US5 - Database (Phase 6)**: Depends on Foundational - No dependencies on other stories (independent P2)
- **US4 - Settings (Phase 7)**: Depends on Foundational + US5 (requires database for persistence)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependency Graph

```text
Foundational (Phase 2)
├── US1 - Shell (Phase 3) ─── independent, no story deps
├── US2 - Theme (Phase 4) ─── runtime independent; persistence bridges to US4
├── US3 - Language (Phase 5) ─ runtime independent; persistence bridges to US4
├── US5 - Database (Phase 6) ─ independent, no story deps
│   └── US4 - Settings (Phase 7) ─ depends on US5
└── Polish (Phase 8) ─ all stories complete
```

### Within Each User Story

- Core implementation before integration
- Interfaces before services
- Services before UI wiring
- Story complete before moving to next phase

### Parallel Opportunities

- All Setup tasks marked [P] (T002, T003, T004) can run in parallel
- All Foundational tasks marked [P] (T005) can run in parallel
- All US1 tasks marked [P] (T009, T010, T011) can run in parallel
- All US2 tasks marked [P] (T017, T018, T019, T020) can run in parallel
- All US3 tasks marked [P] (T024, T025, T026) can run in parallel
- All US5 tasks marked [P] (T031, T032, T033, T034) can run in parallel
- All US4 tasks marked [P] (T039, T040) can run in parallel
- All Polish tasks marked [P] (T045, T046, T047, T048) can run in parallel
- **Cross-story parallelism**: US1, US2, US3, and US5 can all be implemented in parallel once Foundational completes

---

## Parallel Example: User Story 1 (Shell)

```bash
# Launch all [P] US1 tasks together:
Task: "Create INavigationService interface in Planova.Shared/Abstractions/INavigationService.cs"
Task: "Create ShellView.xaml in Planova.UI/Views/ShellView.xaml"
Task: "Create ShellViewModel.cs in Planova.UI/ViewModels/ShellViewModel.cs"

# Then sequential dependent tasks:
Task: "Implement NavigationService in Planova.Application/Services/NavigationService.cs"
Task: "Create NavigationRail control in Planova.UI/Views/NavigationRail.xaml"
Task: "Create WorkspaceArea in Planova.UI/Views/WorkspaceArea.xaml"
```

## Parallel Example: User Story 2 (Theme)

```bash
# Launch all [P] US2 tasks together:
Task: "Create IThemeService interface"
Task: "Create DarkTheme.xaml resource dictionary"
Task: "Create LightTheme.xaml resource dictionary"
Task: "Create ThemeTokens.xaml shared resource dictionary"

# Then sequential:
Task: "Implement ThemeService"
Task: "Add theme toggle UI"
Task: "Audit XAML for hardcoded colors"
```

## Parallel Example: User Story 3 (Language)

```bash
# Launch all [P] US3 tasks together:
Task: "Create ILocalizationService interface"
Task: "Create English resource file"
Task: "Create Arabic resource file"

# Then sequential:
Task: "Implement LocalizationService"
Task: "Add language switcher UI"
Task: "Implement RTL layout switching"
Task: "Ensure all XAML uses resource-backed strings"
```

## Parallel Example: Cross-Story Parallel

With multiple developers after Foundational completes:

```bash
Developer A: US1 - Shell (Phase 3)
Developer B: US2 - Theme (Phase 4)
Developer C: US3 - Language (Phase 5)
Developer D: US5 - Database (Phase 6) → then US4 - Settings (Phase 7)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Shell)
4. **STOP and VALIDATE**: Launch app, verify shell window with navigation rail and workspace
5. Deploy/demo if ready - this is the runnable shell MVP

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Shell) → Test independently → **MVP: Runnable shell with navigation!**
3. Add US2 (Theme) → Test runtime switching → Deploy/Demo
4. Add US3 (Language) → Test runtime switching → Deploy/Demo
5. Add US5 (Database) → Test DB creation → Deploy/Demo
6. Add US4 (Settings) → Test persistence across restarts → **Phase 0 Complete!**
7. Each increment adds value without breaking previous increments

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done (Phase 2):
   - Developer A: US1 - Shell (8 tasks)
   - Developer B: US2 - Theme (7 tasks)
   - Developer C: US3 - Language (7 tasks)
   - Developer D: US5 - Database (8 tasks) → then US4 - Settings (6 tasks)
3. All stories complete and integrate independently
4. Polish (Phase 8) done by any available developer

---

## Notes

- [P] tasks = different files, no dependencies - safe to parallelize
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- US2/US3 runtime switching works without US4; persistence across restarts requires US4
- No test tasks generated (not requested in spec)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
