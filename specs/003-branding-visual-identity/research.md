# Research: Branding Visual Identity

**Feature**: `003-branding-visual-identity`
**Date**: 2026-06-01

## Research Tasks

### R1: Fluent UI Icon Integration for WPF

**Decision**: Replace `FluentWPF 0.10.2` with `Wpf.Ui` (Fluent UI for WPF) to gain access to Fluent UI Icons, modern control styling, and theme system integration.

**Rationale**: 
- The current `FluentWPF 0.10.2` package is referenced but never consumed in any XAML file — it adds bloat with zero benefit.
- `Wpf.Ui` (formerly Wpf.Ui, by lepoco) provides: Fluent UI Icons (`SymbolIcon` with `SymbolRegular` enum covering 1500+ icons), modern theme system (dark/light/high-contrast), `NavigationFluent` control, and `TitleBar` for custom window chrome.
- The spec requires 17 navigation icons as Fluent UI glyphs. `Wpf.Ui.SymbolRegular` provides semantic icon coverage for all 17 items (Dashboard→Home, Projects→Folder, BOQ→DocumentBulletList, etc.).
- `Wpf.Ui` supports `ThemeService` with automatic dark/light/high-contrast detection, replacing the manual resource dictionary swap currently in `ShellViewModel`.

**Alternatives considered**:
- **Segoe Fluent Icons font** (direct XAML FontFamily): Requires manual glyph code mapping, no intellisense, no semantic names. High maintenance burden.
- **Fluent UI System Icons SVGs embedded**: No WPF-native rendering, requires SVG-to-DrawingImage conversion pipeline. Overkill for 17 icons.
- **Keep FluentWPF 0.10.2 + Segoe MDL2**: MDL2 lacks many concept icons (BOQ, WBS, Chronology). Violates spec requirement for Fluent UI icon glyphs.

### R2: Theme Token Architecture — Resource Dictionary vs Wpf.Ui ThemeService

**Decision**: Use `Wpf.Ui.ThemeService` for theme switching (dark/light/high-contrast) while keeping custom `BrandColors.xaml` and `BrandSpacing.xaml` for Planova-specific palette overrides applied on top.

**Rationale**:
- `Wpf.Ui.ThemeService` handles OS theme detection, manual switching, and high-contrast fallback natively — removing manual `ResourceDictionary` swap logic.
- Custom brand tokens (the authoritative hex palette) are applied as a merged dictionary layer on top of the Wpf.Ui base theme. This preserves the "source values are authoritative, contrast overrides create new tokens" rule from the spec clarification.
- High-contrast mode: `Wpf.Ui.ThemeService` fires `ThemeChanged` event. When `SystemParameters.HighContrast` is true, the UI should remove the merged `BrandColors.xaml` and fall back to system colors only — matching the spec requirement "suppress brand colors entirely."

**Alternatives considered**:
- **Pure manual ResourceDictionary swap**: Already implemented but doesn't detect OS high-contrast mode. Would require custom `SystemParameters.HighContrast` listener. More code for less benefit.
- **Avalonia.UI**: Cross-platform framework, violates constitution (WPF required).

### R3: Inter Font Bundling Strategy

**Decision**: Bundle Inter Variable (`.ttf`) as embedded resources in `Planova.UI`, load via `FontFamily` pack URI in XAML, and set as fallback in the `FontFamily` property cascade.

**Rationale**:
- Spec requires Inter as bundled runtime fallback with Segoe UI Variable as primary.
- WPF supports embedded font resources via `pack://application:,,,/Resources/Fonts/Inter/#Inter` URI syntax.
- Variable fonts (`Inter-Variable.ttf`) provide weight and width axes in a single file, reducing bundle size vs. separate static font files.
- Inter is licensed under SIL Open Font License 1.1 — free to bundle and distribute.

**Implementation details**:
1. Add `Inter-Variable.ttf` and `Inter-Italic-Variable.ttf` to `Resources/Branding/Inter/`
2. Set build action to `Resource` (not EmbeddedResource — WPF uses `Resource` for pack URI access)
3. Define `FontFamily` in `ThemeTokens.xaml`: `<FontFamily x:Key="BrandFontFamily">Segoe UI Variable, pack://application:,,,/Resources/Branding/Inter/#Inter</FontFamily>`
4. Apply to all text elements via style setters

**Alternatives considered**:
- **System-installed Inter**: Spec explicitly requires bundling for guaranteed rendering.
- **Static font files (weight-specific)**: 8+ files vs. 2 variable files. More maintenance, larger bundle.

### R4: Adaptive Gradient Background Implementation

**Decision**: Implement shell background gradients using `LinearGradientBrush` defined in `Gradients.xaml` as theme-aware resources, switched via `Wpf.Ui.ThemeService.ThemeChanged`.

**Rationale**:
- Spec clarification requires "adaptive gradient that shifts based on active theme and content area."
- WPF `LinearGradientBrush` supports `GradientStop` collections that can be defined per theme as named resources.
- Gradients shift between: Dark theme (deep `#0D1117` → `#161B22`) and Light theme (soft `#F8F9FB` → white with subtle accent tint).
- Content-area adaptation: Dashboard gets a deeper gradient, detail workspaces get a flatter gradient. Achieved via `x:Key` variants (`ShellBackgroundGradient`, `DashboardBackgroundGradient`, `WorkspaceBackgroundGradient`).

**Implementation**:
1. Create `Styles/Gradients.xaml` with `LinearGradientBrush` definitions per theme per area
2. Merge into theme dictionaries
3. Apply `Background="{DynamicResource ShellBackgroundGradient}"` to shell root and content area elements
4. High-contrast: fall back to solid `SystemColors.WindowBrush`

**Alternatives considered**:
- **RadialGradientBrush**: Inconsistent with reference design (linear depth).
- **Acrylic/Mica backdrop**: Performance-heavy for WPF. Not matching the reference visual.

### R5: Window Title Bar Branding (Non-Client Area)

**Decision**: Use `Wpf.Ui.TitleBar` control for custom window chrome that places the Planova logo in the title bar area.

**Rationale**:
- `Wpf.Ui.TitleBar` provides a WPF-native title bar with icon, title, and window control integration.
- Allows inserting a logo `Image` in the title bar `Icon` or `Header` slot.
- Avoids Win32 `SendMessage`/`DwmSetWindowAttribute` P/Invoke complexity for non-client area icon injection.
- Complies with the spec clarification: "Add the Planova logo to the app title bar (window chrome)."

**Alternatives considered**:
- **P/Invoke DwmSetWindowAttribute**: Windows 11 only, fragile, doesn't support WPF rendering in non-client area.
- **Borderless Window + manual hit-test**: Complex, error-prone, accessibility issues with screen readers. Wpf.Ui already solves this.

### R6: High-Contrast Mode Detection and Fallback

**Decision**: Create `HighContrastDetector` service that listens to `SystemParameters.HighContrast` changes via `System.Windows.SystemParameters.HighContrastProperty` and `Wpf.Ui.ThemeService.ThemeChanged` to replace all brand dictionaries with system colors.

**Rationale**:
- Spec requires: "OS high-contrast theme fully overrides all surface and text colors."
- `SystemParameters.HighContrast` property changes when user toggles high-contrast in Windows Settings.
- When detected: remove `BrandColors.xaml` and `Gradients.xaml` from merged dictionaries, add `HighContrastFallback.xaml` that maps all brand tokens to `SystemColors` equivalents.
- When high-contrast exits: restore brand dictionaries and active theme.

**Implementation**:
```csharp
// HighContrastDetector.cs
public class HighContrastDetector
{
    private readonly IThemeService _themeService;
    
    public void WatchForChanges()
    {
        SystemParameters.StaticPropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "HighContrast")
                ApplyHighContrastFallback(SystemParameters.HighContrast);
        };
    }
}
```

### R7: Navigation Item Icon Mapping (17 Items)

**Decision**: Map all 17 navigation items to `Wpf.Ui.SymbolRegular` enum values for consistent Fluent UI icon glyphs.

**Mapping**:
| Nav Item | SymbolRegular | Glyph |
|----------|---------------|-------|
| Dashboard | Home | \uE80F-equivalent |
| Projects | Folder | concept |
| BOQ Studio | DocumentBulletList | list/items concept |
| WBS Studio | TreeView | hierarchy concept |
| Activity Studio | CalendarDay | scheduling concept |
| Resource Studio | People | team concept |
| Cost Studio | Money | cost concept |
| Reports | DocumentBar | reporting concept |
| Primavera Studio | HardDrive | integration concept |
| Schedule Compare | ArrowSync | comparison concept |
| Delay Analysis | ChartMultiple | analysis concept |
| Claims | DocumentEdit | claims concept |
| Chronology | Timeline | chronology concept |
| Correspondence | Mail | correspondence concept |
| Knowledge Base | BookSearch | knowledge concept |
| Analytics | DataHistogram | analytics concept |
| Integration Hub | PlugConnected | integration concept |
| Settings | Settings | settings concept |

Exact `SymbolRegular` enum values will be verified during implementation against the Wpf.Ui API documentation for the installed version.

### R8: AI Assistant Panel Default State

**Decision**: Panel starts collapsed (width 0 or margin-collapsed) with a branded indicator icon strip (48px wide rail with AI icon). User click expands to full 320px panel.

**Rationale**:
- Spec clarification: "Start collapsed by default, expand on user action; branding applies to both collapsed indicator and expanded panel."
- Matches design system: "Persistent Across Modules" and "Collapsible."
- The existing `AIAssistantPanelStyle` in `BrandStyles.xaml` defines 320px width — this becomes the expanded width.
- Collapsed state: 48px icon strip with Fluent UI `Bot` icon, matching nav rail rhythm.

### R9: Window Title Bar Implementation Approach

**Decision**: Use `Wpf.Ui.TitleBar` as the title bar chrome with logo placement in the `Header` template area, integrated with the existing shell header composition.

**Rationale**:
- Pure WPF approach that avoids platform invokes
- Logo visibility in title bar matches spec requirement FR-001 for window chrome logo
- The `TitleBar` control provides resize handles, window commands (minimize/maximize/close), and drag behavior out of the box
- Logo image placed in the `Header`/`Icon` slot of TitleBar, which sits in the same row as tabs per the implementation plan

**Alternatives considered**:
- **Custom Window.Chrome**: Requires manual hit-testing, resize grips, and a lot of code-behind for standard window behavior. Wpf.Ui.TitleBar handles this.
- **Standard title bar**: Cannot render logo or custom chrome. Violates FR-001.

## Resolved NEEDS CLARIFICATION Items

All technical unknowns from the initial plan template have been resolved through the research above. No NEEDS CLARIFICATION items remain.