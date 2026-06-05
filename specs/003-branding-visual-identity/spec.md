# Feature Specification: Branding Visual Identity

**Feature Branch**: `003-branding-visual-identity`

**Created**: 2026-06-01

**Status**: Draft

**Input**: User description: "Post-Phase 0 visual identity pass that applies the Planova brand system to the application shell, navigation rail, dashboard, workspace chrome, and AI assistant panel, including logo placement, iconography, spacing tokens, and dark/light theme fidelity"

## Clarifications

### Session 2026-06-01

- Q: What minimum accessibility contrast ratio should branded elements meet? → A: WCAG AA minimum (4.5:1 normal text, 3:1 large text)
- Q: How should logos and wordmarks behave when the application is in Arabic/RTL mode? → A: Logo and wordmark stay left-anchored; navigation chrome mirrors for RTL
- Q: Should navigation icons use Fluent UI icon glyphs or PNG bitmaps? → A: Fluent UI icon glyphs, no PNG bitmaps.
- Q: Should the application window title bar display the Planova logo in addition to the shell header? → A: Yes, add the Planova logo to the app title bar (window chrome).
- Q: How should brand elements behave when Windows high-contrast mode is active? → A: Suppress brand colors entirely — OS high-contrast theme fully overrides all surface and text colors.
- Q: What visual should display when a brand asset file is missing? → A: Show a generic Fluent UI placeholder icon in place of the missing asset.
- Q: What level of visual treatment should empty states receive? → A: Branded Fluent UI illustration icon plus descriptive text with brand surface treatment.
- Q: Which navigation item set should the branding pass implement — the 11 items in the implementation plan or the full 17 items from the design system? → A: All 17 navigation items from the design system (future-proof full set).
- Q: Should the design system hex palette values be used as exact authoritative tokens or as starting points adjustable for contrast? → A: Use exact hex values as authoritative; adjustments for contrast create new token overrides, not source changes.
- Q: Should the AI assistant panel always be visible as an open panel by default or start collapsed? → A: Start collapsed by default, expand on user action; branding applies to both collapsed indicator and expanded panel.
- Q: Should the shell background use a constant gradient or an adaptive gradient that shifts by theme/content area? → A: Adaptive gradient that shifts based on active theme or content area.
- Q: Should Inter be bundled as a runtime fallback font or rely on system installation? → A: Bundle Inter as a runtime fallback font shipped with the application (guaranteed consistent rendering).

## User Scenarios & Testing

### User Story 1 - Experience Branded Application Shell (Priority: P1)

A user launching the application immediately sees the Planova brand identity through logo placement, branded navigation rail, and consistent visual language.

**Why this priority**: Brand identity is the first thing users see. It establishes product credibility and sets the visual foundation that every later module inherits.

**Independent Test**: Can be fully tested by launching the application and visually verifying the logo, wordmark, navigation icons, and shell header match the brand reference assets.

**Acceptance Scenarios**:

1. **Given** the user launches the application, **When** the shell window opens, **Then** the Planova logo and wordmark are visible in the top-left header area.
2. **Given** the shell is displayed, **When** the user views the navigation rail, **Then** all navigation items have branded icons with consistent style and selected-state treatment.
3. **Given** the branded shell is displayed, **When** the user compares it to the brand reference assets, **Then** the spacing, surfaces, and composition match the intended design.

---

### User Story 2 - Experience Branded Dashboard (Priority: P1)

A user viewing the dashboard sees branded KPI cards, quick action tiles, and section hierarchy that reflects the Planova product identity.

**Why this priority**: The dashboard is the primary landing surface. Branded presentation here establishes the product feel for the most visible workspace.

**Independent Test**: Can be fully tested by opening the dashboard and verifying that KPI cards, action buttons, and section headers use branded styling consistent with the reference assets.

**Acceptance Scenarios**:

1. **Given** the user opens the dashboard, **When** the workspace loads, **Then** KPI cards display with branded surfaces, spacing, and typography.
2. **Given** dashboard cards are displayed, **When** the user views quick action buttons, **Then** they show branded iconography and consistent styling.
3. **Given** dashboard sections are rendered, **When** the user scrolls through the workspace, **Then** section hierarchy and spacing follow the brand reference layout.

---

### User Story 3 - Switch Between Dark and Light Branded Themes (Priority: P2)

A user switching between dark and light themes sees both modes render with the same brand identity, not just minimum accessible contrast.

**Why this priority**: Theme fidelity ensures the brand is recognizable in both modes, which is critical for users who work in different lighting environments.

**Independent Test**: Can be fully tested by switching between dark and light themes and comparing both modes against the brand reference assets for logo visibility, surface colors, and overall brand cohesion.

**Acceptance Scenarios**:

1. **Given** the user switches from dark to light theme, **When** the shell re-renders, **Then** logo and icon variants update to match the active theme.
2. **Given** either theme is active, **When** the user views any shell surface, **Then** the surface colors, borders, and elevations feel like the same brand family.
3. **Given** the user views the dashboard in both themes, **When** they compare KPI cards and panels, **Then** the visual hierarchy and brand treatment remain consistent across modes.

---

### Edge Cases

- What happens when brand asset files are missing or corrupted? The application should show a generic Fluent UI placeholder icon in place of the missing asset and log an asset-loading warning.
- How does the application handle extremely small window sizes where the logo or wordmark would overlap? Brand elements should have minimum size thresholds and collapse gracefully (e.g., wordmark hides, logo-only mode activates).
- What happens when the user has a custom Windows theme or high-contrast mode active? The application theme system must fully suppress brand colors and let OS high-contrast theme override all surface and text colors. No custom brand styling in high-contrast mode.
- How does the system ensure branded text and UI elements meet accessibility contrast standards? All branded text and interactive elements must meet WCAG AA minimum contrast ratios (4.5:1 for normal text, 3:1 for large text).
- How should brand elements render when the application switches to Arabic/RTL layout? Logo and wordmark remain left-anchored; navigation chrome mirrors to respect RTL reading direction.

## Requirements

### Functional Requirements

- **FR-001**: System MUST display the Planova logo in the window title bar (non-client area) and the logo plus wordmark in the shell header area on application launch.
- **FR-002**: System MUST apply a consistent set of navigation icons across all 17 rail items (Dashboard, Projects, BOQ Studio, WBS Studio, Activity Studio, Resource Studio, Cost Studio, Reports, Primavera Studio, Schedule Compare, Delay Analysis, Claims, Chronology, Correspondence, Knowledge Base, Analytics, Integration Hub, Settings), with selected and hover state styling.
- **FR-003**: System MUST render dashboard KPI cards with branded surface colors, spacing, and typography.
- **FR-004**: System MUST apply branded styling to workspace tabs, including active-state and inactive-state treatment.
- **FR-005**: System MUST apply branded visual treatment to the AI assistant panel surface and chrome, including a branded collapsed indicator (default state) and full branded panel when expanded by user action.
- **FR-006**: System MUST support theme-specific logo variants (light-mode logo for dark theme, dark-mode logo for light theme).
- **FR-007**: System MUST apply consistent spacing, border, elevation, and surface tokens across all shell areas, using the authoritative design system palette (Dark: Background `#0D1117`, Surface `#161B22`, Card `#1F2937`, Border `#2A3441`, Primary `#00BFFF`, Secondary `#0078D4`; Light: Background `#F8F9FB`, Surface `#FFFFFF`, Card `#FFFFFF`, Border `#D6DCE5`, Primary `#0078D4`, Secondary `#00BFFF`) as source values. Contrast adjustments MUST create new token overrides rather than modify source values.
- **FR-010**: System MUST render shell backgrounds using an adaptive gradient that shifts based on the active theme and content area, avoiding dead-flat monochrome backgrounds while preserving legibility and contrast.
- **FR-008**: System MUST render the branded visual identity in both dark and light themes with cohesive brand family appearance.
- **FR-009**: System MUST apply branded empty-state and placeholder styling for workspaces with no data, including a branded Fluent UI illustration icon and descriptive text on a branded surface.
- **FR-011**: System MUST use Segoe UI Variable as the primary font with Inter bundled as a runtime fallback font shipped with the application, ensuring consistent typography rendering across all machines.

### Key Entities

- **Brand Asset**: Visual source material (logos, icons, reference composites) stored in the brand master folder, used as reference for implementation.
- **Theme Token**: Named visual property (surface color, border color, spacing value, elevation level, font size) that controls the appearance of shell areas. Tokens have dark-mode and light-mode values. Authority palette: Dark theme — Background `#0D1117`, Surface `#161B22`, Card `#1F2937`, Border `#2A3441`, Primary Accent `#00BFFF`, Secondary Accent `#0078D4`; Light theme — Background `#F8F9FB`, Surface `#FFFFFF`, Card `#FFFFFF`, Border `#D6DCE5`, Primary Accent `#0078D4`, Secondary Accent `#00BFFF`. Contrast overrides produce new tokens rather than modifying these source values.
- **Icon**: Navigation and action icon rendered as Fluent UI icon glyph with consistent style, supporting selected, hover, and default visual states.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A user can identify the application as Planova within 3 seconds of first launch, based on logo, wordmark, and shell composition.
- **SC-002**: All navigation rail items display branded icons with consistent styling — no mixed icon sources or unthemed default icons remain.
- **SC-003**: Dashboard KPI cards and action tiles match the brand reference layout within 90% visual fidelity (spacing, surface treatment, hierarchy).
- **SC-004**: Dark and light themes both render with brand-cohesive appearance — switching between modes does not produce a visually disconnected experience.
- **SC-005**: Empty states and placeholders across all current workspaces display branded styling, not generic system defaults.
- **SC-006**: Theme fidelity comparison between the implemented shell and the brand reference assets shows no more than 5 visual discrepancies that affect brand recognition.
- **SC-007**: All branded text and interactive elements meet WCAG AA minimum contrast ratios (verified by automated contrast checking tool).

## Assumptions

- Brand asset files (logos, icons, reference composites) are available in the existing `Planova Branding Master` folder and the `docs/03-BRAND_GUIDELINES.md` document.
- The existing Phase 0 shell, navigation, and theme system are stable and working — this pass modifies visual properties only, not structural behavior.
- The application targets desktop resolutions only — responsive or mobile layout is out of scope for this branding pass.
- Iconography uses Fluent UI icon glyphs (existing project dependency); no PNG bitmap icons and no new icon font or external icon library is introduced.
- Theme tokens are expressed as resource values that can be updated without modifying view code.
- No new business features, domain logic, database changes, or API integrations are introduced by this branding pass.
- The branding treatment applied to the shell, dashboard, and workspace chrome will be inherited by future studio modules without rework.
- Typographic consistency is guaranteed by bundling Inter as a runtime fallback font; Segoe UI Variable is the primary font, Inter is the bundled fallback.
