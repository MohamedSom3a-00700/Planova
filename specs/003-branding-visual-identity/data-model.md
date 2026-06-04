# Data Model: Branding Visual Identity

**Feature**: `003-branding-visual-identity`
**Date**: 2026-06-01

> Note: This branding pass introduces no new domain entities, database changes, or persistence models. The "data model" here describes the visual/token model that governs the branding system — the resource dictionaries, theme tokens, and configuration that will be implemented in XAML and C#.

## Entities

### ThemeToken

Named visual property that controls the appearance of shell areas. Each token has a dark-mode and light-mode value. The authoritative palette is fixed per the spec clarification; contrast overrides produce new tokens.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Key | `string` | Resource dictionary key (e.g., `BrandBackgroundBrush`) | Must be unique across all theme dictionaries |
| Category | `TokenCategory` | Classification: Color, Spacing, Radius, Elevation, Font | — |
| DarkValue | `string` | Hex color, double (spacing), or FontFamily | Must match authoritative dark palette values |
| LightValue | `string` | Hex color, double (spacing), or FontFamily | Must match authoritative light palette values |
| HighContrastValue | `string` | Fallback value when OS high-contrast is active | Must map to `SystemColors` or high-contrast equivalents |
| IsContrastOverride | `bool` | Whether this token is a contrast-derived override of a source token | If `true`, source token value is preserved |

**Authoritative Source Palette (immutable)**:

| Token Key | Category | Dark Value | Light Value |
|-----------|----------|------------|-------------|
| `ThemeBackground` | Color | `#0D1117` | `#F8F9FB` |
| `ThemeSurface` | Color | `#161B22` | `#FFFFFF` |
| `ThemeCard` | Color | `#1F2937` | `#FFFFFF` |
| `ThemeBorder` | Color | `#2A3441` | `#D6DCE5` |
| `ThemeAccent` | Color | `#00BFFF` | `#0078D4` |
| `ThemeSecondaryAccent` | Color | `#0078D4` | `#00BFFF` |

**Extended Brand Palette (supporting tokens)**:

| Token Key | Category | Dark Value | Light Value |
|-----------|----------|------------|-------------|
| `BrandElevationLow` | Color | `#2A2A3E` | `#FFFFFF` |
| `BrandElevationMedium` | Color | `#32324A` | `#F0F0F0` |
| `BrandHeaderForeground` | Color | `#E0E0E0` | `#1A1A1A` |
| `BrandTextSecondary` | Color | `#9E9E9E` | `#5C5C5C` |
| `BrandSuccess` | Color | `#2ECC71` | `#2ECC71` |
| `BrandWarning` | Color | `#F39C12` | `#F39C12` |
| `BrandDanger` | Color | `#E74C3C` | `#E74C3C` |
| `BrandAccentHover` | Color | `#106EBE` | `#106EBE` |
| `BrandAccentPressed` | Color | `#005A9E` | `#005A9E` |

**Spacing Tokens (shared across themes)**:

| Token Key | Value |
|-----------|-------|
| `BrandSpacingXSmall` | 4 |
| `BrandSpacingSmall` | 8 |
| `BrandSpacingMedium` | 16 |
| `BrandSpacingLarge` | 24 |
| `BrandSpacingXLarge` | 32 |
| `BrandSpacingXXLarge` | 48 |

**Typography Tokens**:

| Token Key | Value |
|-----------|-------|
| `BrandFontFamily` | `Segoe UI Variable, pack://application:,,,/Resources/Branding/Inter/#Inter` |
| `BrandHeaderSize` | 24 |
| `BrandSubheaderSize` | 18 |
| `BrandBodySize` | 14 |
| `BrandLabelSize` | 12 |
| `BrandCaptionSize` | 11 |

### NavigationItem

Represents a single entry in the navigation rail.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | `string` | Navigation target identifier (e.g., `"dashboard"`) | Must match INavigationService registration |
| DisplayName | `string` | Localized display label | Must be localizable (en/ar) |
| Icon | `SymbolRegular` | Fluent UI icon glyph enum value | Must exist in Wpf.Ui SymbolRegular |
| Order | `int` | Display order in navigation rail | Unique per item |
| IsStudio | `bool` | Whether this item represents a studio module | — |
| IsPlaceholder | `bool` | Whether the target module is not yet implemented | Placeholder items show branded empty state |

**Registered Items (17 total)**:

| Order | Id | DisplayName | Icon (SymbolRegular) | IsStudio | IsPlaceholder |
|-------|----|-------------|---------------------|----------|---------------|
| 1 | dashboard | Dashboard | Home | false | false |
| 2 | projects | Projects | Folder | false | false |
| 3 | boq | BOQ Studio | DocumentBulletList | true | false |
| 4 | wbs | WBS Studio | TreeView | true | true |
| 5 | activity | Activity Studio | CalendarDay | true | true |
| 6 | resource | Resource Studio | People | true | true |
| 7 | cost | Cost Studio | Money | true | true |
| 8 | reports | Reports | DocumentBar | false | false |
| 9 | primavera | Primavera Studio | HardDrive | true | true |
| 10 | schedule-compare | Schedule Compare | ArrowSync | true | true |
| 11 | delay-analysis | Delay Analysis | ChartMultiple | true | true |
| 12 | claims | Claims | DocumentEdit | false | false |
| 13 | chronology | Chronology | Timeline | true | true |
| 14 | correspondence | Correspondence | Mail | false | true |
| 15 | knowledge-base | Knowledge Base | BookSearch | true | true |
| 16 | analytics | Analytics | DataHistogram | false | true |
| 17 | integration-hub | Integration Hub | PlugConnected | true | true |
| 18 | settings | Settings | Settings | false | false |

### GradientBrush

Adaptive gradient definition per theme per surface area.

| Field | Type | Description |
|-------|------|-------------|
| Key | `string` | Resource key (e.g., `ShellBackgroundGradient`) |
| Theme | `ThemeMode` | Dark or Light |
| Stops | `GradientStop[]` | Ordered color stops with offset |
| StartPoint | `Point` | Gradient start point (relative) |
| EndPoint | `Point` | Gradient end point (relative) |

**Defined Gradient Brushes**:

| Key | Dark Stops | Light Stops |
|-----|-----------|-------------|
| `ShellBackgroundGradient` | `#0D1117`(0.0) → `#161B22`(1.0) | `#F8F9FB`(0.0) → `#FFFFFF`(1.0) |
| `DashboardBackgroundGradient` | `#0D1117`(0.0) → `#1F2937`(0.6) → `#161B22`(1.0) | `#F8F9FB`(0.0) → `#F0F0F0`(0.4) → `#FFFFFF`(1.0) |
| `WorkspaceBackgroundGradient` | `#161B22`(0.0) → `#1F2937`(1.0) | `#FFFFFF`(0.0) → `#F8F9FB`(1.0) |

High-contrast fallback: All gradients replaced by `SystemColors.WindowBrush`.

### BrandAsset

Visual source file reference used by the theme system.

| Field | Type | Description |
|-------|------|-------------|
| Key | `string` | Resource key (e.g., `LogoDark`) |
| FilePath | `string` | Pack URI path to embedded resource |
| ThemeMode | `ThemeMode?` | null = universal; Dark/Light = theme-specific |
| MinWidth | `double` | Minimum render width before collapse |
| MinHeight | `double` | Minimum render height |

**Brand Assets**:

| Key | FilePath | ThemeMode | Notes |
|-----|----------|-----------|-------|
| LogoDark | `pack://.../Branding/LogoDark.png` | Dark | Dark variant for dark theme header |
| LogoLight | `pack://.../Branding/LogoLight.png` | Light | Light variant for light theme header |
| LogoMonochrome | `pack://.../Branding/LogoMonochrome.png` | null | Universal variant for fallback |
| Wordmark | `pack://.../Branding/Wordmark.png` | null | Hides below 640px width |
| InterFont | `pack://.../Branding/Inter/#Inter` | null | Bundled variable font family |

### VisualState

Interactive states for branded controls.

| State | Properties |
|-------|-----------|
| Normal | Default token values |
| Hover | Accent overlay at 10% opacity, background lightens/darkens by 1 shade |
| Pressed | Accent overlay at 20% opacity, background reduces elevation |
| Selected | Left accent border stripe (2px), background uses selected token |
| Disabled | 40% opacity, no accent colors |
| Focus | 2px accent border outline (keyboard navigation only) |

Applies to: Navigation items, action buttons, quick actions, tabs, header controls.

## Relationships

```
ThemeToken ──defines──> GradientBrush
ThemeToken ──defines──> BrandAsset (color context)
NavigationItem ──uses──> ThemeToken (via DynamicResource)
NavigationItem ──uses──> BrandAsset (via Icon)
GradientBrush ──uses──> ThemeToken (color values)
BrandAsset ──switches──> ThemeMode
```

## Validation Rules

1. All `ThemeToken` Dark/Light values MUST match the authoritative palette hex values exactly.
2. No `ThemeToken` may have the same Key as a `SystemColors` property (avoid collision).
3. All `NavigationItem` IDs must exist in `INavigationService` registration.
4. Placeholder `NavigationItem` targets MUST render branded empty-state styling.
5. High-contrast mode MUST suppress all brand tokens and replace them with `SystemColors`.
6. Font fallback chain: Segoe UI Variable → Inter (bundled) → system sans-serif.
7. All gradient stops MUST meet WCAG AA contrast ratio against text rendered on them.