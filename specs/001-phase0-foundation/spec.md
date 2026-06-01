# Feature Specification: Planova Phase 0 Foundation

**Feature Branch**: `001-phase0-foundation`

**Created**: 2026-06-01

**Status**: Draft

**Input**: User description: "Implement Planova Phase 0 foundation: desktop shell, navigation rail, theme system, localization (EN/AR), logging, settings, and persistence baseline"

## Clarifications

### Session 2026-06-01

- Q: Security & Privacy Baseline → A: Local-only with no auth, database not encrypted; deferred to later phases
- Q: Edge Case Behavior → A: Show user-friendly error dialog, log full details internally; allow retry or graceful exit
- Q: Observability / Logging Scope → A: Info-level for startup/settings/db lifecycle + errors; retain 7 days or 100 MB

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Application Launch and Shell (Priority: P1)

As a user, I want the application to start cleanly and display a working shell window with navigation so that I can begin using the product.

**Why this priority**: Every other feature depends on the shell being functional. Without a working launch, nothing else can be tested.

**Independent Test**: Can be fully tested by launching the application and verifying the shell window appears with a navigation rail, multi-tab workspace, and status area.

**Acceptance Scenarios**:

1. **Given** the application is installed, **When** the user launches it, **Then** the shell window opens within 5 seconds with no errors
2. **Given** the shell window is open, **When** the user views the interface, **Then** a navigation rail is displayed on the left and a multi-tab workspace is displayed in the center
3. **Given** the shell is displayed, **When** the user clicks a navigation rail item, **Then** a corresponding tab opens in the workspace area

---

### User Story 2 - Theme Switching (Priority: P1)

As a user, I want to switch between dark and light themes at runtime so that I can work comfortably in different lighting conditions.

**Why this priority**: Theme affects the entire visual experience and must work from day one to avoid hardcoded color retrofits.

**Independent Test**: Can be fully tested by launching the app, switching from dark to light theme, and verifying all UI elements update immediately.

**Acceptance Scenarios**:

1. **Given** the application is running with dark theme as default, **When** the user selects light theme, **Then** all UI elements switch to light colors without visual artifacts
2. **Given** the user has selected a theme, **When** the application is restarted, **Then** the previously selected theme is restored
3. **Given** any theme is active, **When** the user inspects the UI, **Then** no hardcoded colors are used where theme resources are available

---

### User Story 3 - Language Switching (Priority: P1)

As a user, I want to switch between English and Arabic at runtime with correct RTL layout so that I can use the application in my preferred language.

**Why this priority**: The application targets bilingual users. Localization must be first-class from the start, not retrofitted later.

**Independent Test**: Can be fully tested by launching the app, switching language from English to Arabic, and verifying all shell labels update and layout flows right-to-left.

**Acceptance Scenarios**:

1. **Given** the application is running in English, **When** the user switches to Arabic, **Then** all user-facing shell strings display in Arabic
2. **Given** Arabic is selected, **When** the user views the interface, **Then** the layout renders correctly in right-to-left mode
3. **Given** the user switches language, **When** the application is restarted, **Then** the selected language persists

---

### User Story 4 - Settings Persistence (Priority: P2)

As a user, I want my preferences (theme, language, window state) to survive application restarts so that I don't have to reconfigure every time.

**Why this priority**: Settings persistence is foundational for user trust and a polished experience, but the app remains testable without it.

**Independent Test**: Can be fully tested by changing settings, restarting the app, and verifying settings are restored.

**Acceptance Scenarios**:

1. **Given** the user has configured theme and language preferences, **When** the application restarts, **Then** those preferences are applied automatically
2. **Given** the user resizes or repositions the window, **When** the application restarts, **Then** the window opens at the saved size and position

---

### User Story 5 - Database Initialization (Priority: P2)

As a developer, I want the database to initialize automatically on first launch so that the application is ready for data persistence without manual setup.

**Why this priority**: Persistence is required by all future modules, but Phase 0 only needs the foundation to be stable.

**Independent Test**: Can be fully tested by launching the app on a clean environment and verifying the database file is created and schema initializes successfully.

**Acceptance Scenarios**:

1. **Given** the application is launched for the first time, **When** the startup sequence completes, **Then** a local database file is created successfully
2. **Given** the database exists, **When** the application starts, **Then** schema updates run without errors

---

### Edge Cases

All edge cases below follow the standard error handling pattern: show a user-friendly error dialog, log full diagnostic details internally, and allow the user to retry or exit gracefully.

- **Restricted file permissions** (cannot create database or settings file): Show error dialog explaining the issue, log the permission path and error details, offer to retry or exit.
- **Corrupted database file** on startup: Show error dialog with option to reset database to clean state or exit; log corruption details for support.
- **Missing or corrupted resource file** (e.g., Arabic language resources): Fall back to default language (English), log the missing resource details, and show a one-time notification.
- **Display configuration changes** (e.g., DPI scaling, monitors): Adapt layout dynamically without crashing; log display change events for diagnostics.
- **Language switch during modal dialog**: Complete the current dialog in the original language; apply language change after the dialog closes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST launch to a working shell window with a collapsible navigation rail on the left and a multi-tab workspace in the center.
- **FR-002**: System MUST support placeholder navigation targets for future studios (BOQ, WBS, Scheduling, Claims, etc.) without implementing their business logic.
- **FR-003**: System MUST provide dark theme as the default and light theme as an option, switchable at runtime without restart.
- **FR-004**: System MUST persist the selected theme preference and restore it on next launch.
- **FR-005**: System MUST provide English and Arabic localization with all user-facing shell strings sourced from resource files.
- **FR-006**: System MUST support runtime language switching that updates all visible UI strings immediately.
- **FR-007**: System MUST support right-to-left (RTL) layout rendering when Arabic is selected.
- **FR-008**: System MUST persist the selected language preference and restore it on next launch.
- **FR-009**: System MUST capture startup, settings-load, database-init, and error events via structured logging at info level; debug-level details are not required for Phase 0.
- **FR-010**: System MUST persist user preferences (theme, language, window state) across application restarts.
- **FR-011**: System MUST initialize a local database on first launch with versioned schema management.
- **FR-012**: System MUST log all unhandled application exceptions with sufficient context for diagnosis; logs must retain a minimum of 7 days or 100 MB (whichever is reached first).
- **FR-013**: System MUST enforce layered architecture dependency rules: outer layers may reference inner layer abstractions, inner layers have no dependencies on outer layers or external frameworks.
- **FR-014**: System MUST separate presentation from business logic, with no business logic embedded in the user interface layer.
- **FR-015**: System MUST NOT require user authentication or data-at-rest encryption for Phase 0; all security and privacy concerns are deferred to later phases when business data entities are introduced.

### Key Entities *(include if feature involves data)*

- **User Preferences**: Represents the user's theme selection (dark/light), language selection (en/ar), and window state (position, size, maximized state). Persisted across restarts.
- **Database Schema Foundation**: The initial schema and versioning infrastructure that supports future entity additions without restructuring.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Application launches to a working shell window within 5 seconds on a standard development workstation.
- **SC-002**: Users can switch between dark and light themes in under 2 seconds with all UI elements updating correctly.
- **SC-003**: Users can switch between English and Arabic in under 2 seconds with all shell strings updating and RTL layout rendering correctly.
- **SC-004**: User preferences (theme, language) survive application restart without loss.
- **SC-005**: Database initializes automatically on first launch without manual setup steps.
- **SC-006**: Structured logging captures startup events, settings load, and database initialization; startup failures are logged with sufficient context.
- **SC-007**: Navigation rail displays at least one working navigation target that opens a tab in the workspace.
- **SC-008**: No Phase 0 work introduces a workflow engine, automation designer, or AI provider coupling.

## Assumptions

- Users have a standard desktop environment capable of running GUI applications.
- Target users are construction planning professionals who work in both English and Arabic.
- The application targets desktop operating systems with standard window management capabilities.
- Future module studios (BOQ, WBS, Scheduling, etc.) will be developed in later phases and will use the navigation/tab infrastructure established in Phase 0.
- The existing product vision and system architecture documents are the authoritative sources for overall application design decisions.
- Standard error handling patterns (user-friendly messages with appropriate fallbacks) are acceptable for Phase 0.
- Settings storage format will support future expansion without breaking existing users.
- User authentication and data-at-rest encryption are out of scope for Phase 0; they will be introduced when business data entities are added in later phases.
