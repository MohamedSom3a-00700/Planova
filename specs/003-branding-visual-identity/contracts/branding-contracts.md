# Contracts: Branding Visual Identity

**Feature**: `003-branding-visual-identity`
**Date**: 2026-06-01

This feature is a UI-layer branding pass with no external API contracts, service contracts, or public interfaces beyond the internal WPF interface changes described below. No REST endpoints, CLI commands, or inter-process contracts are introduced.

## Internal Interface Contracts

### IThemeService (Enhanced)

The existing `IThemeService` in `Planova.Shared` is enhanced to support high-contrast detection and gradient switching.

```csharp
public interface IThemeService
{
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;
    AppTheme CurrentTheme { get; }
    void SetTheme(AppTheme theme);
}

public enum AppTheme
{
    Dark,
    Light,
    HighContrast
}

public class ThemeChangedEventArgs : EventArgs
{
    public AppTheme NewTheme { get; init; }
}
```

**Contract obligations**:
- `SetTheme(AppTheme.HighContrast)` removes all brand color and gradient dictionaries and applies `SystemColors`-based styling.
- `SetTheme(AppTheme.Dark)` applies `DarkTheme.xaml` + `BrandColors.xaml` + `Gradients.xaml`.
- `SetTheme(AppTheme.Light)` applies `LightTheme.xaml` + `BrandColors.xaml` + `Gradients.xaml`.
- `ThemeChanged` event fires after dictionaries are swapped and layout updated.

### INavigationService (Enhanced)

Enhanced to support 17 navigation targets with icon metadata.

```csharp
public sealed record NavigationTargetInfo(
    string Id,
    string DisplayName,
    string IconGlyph,
    bool IsStudio = false,
    bool IsPlaceholder = false
);

public interface INavigationService
{
    event EventHandler<string>? ActiveTargetChanged;
    void NavigateTo(string targetId);
    void RegisterTarget(string id, string displayName, Func<object> viewFactory);
    string GetActiveTarget();
    IReadOnlyCollection<NavigationTargetInfo> GetTargets();
    bool TryCreateView(string targetId, out object? view);
}
```

**Contract obligations**:
- `IconGlyph` must be a valid `Wpf.Ui.SymbolRegular` enum name string.
- Placeholder targets render the branded empty-state view.
- All 17 targets must be registered at application startup.

### IHighContrastDetector (New)

```csharp
public interface IHighContrastDetector
{
    bool IsHighContrast { get; }
    event EventHandler<bool> HighContrastChanged;
}
```

**Contract obligations**:
- `IsHighContrast` reflects `SystemParameters.HighContrast` at all times.
- `HighContrastChanged` fires when OS high-contrast setting changes.
- Implementation listens to `SystemParameters.StaticPropertyChanged` for `HighContrast` property.

## XAML Resource Contracts

### Theme Dictionary Merge Order

The following merge order is a contract for `App.xaml`:

```
1. ThemeTokens.xaml       (base defaults — always merged)
2. BrandStyles.xaml       (named styles — always merged)
3. BrandSpacing.xaml      (spacing/radius/font tokens — always merged)
4. Gradients.xaml         (adaptive gradients — always merged except HighContrast)
5. BrandColors.xaml        (brand color palette — merged for Dark/Light, removed for HighContrast)
6. DarkTheme.xaml         (dark overrides) OR LightTheme.xaml (light overrides) OR HighContrastFallback.xaml
```

### Named Style Keys (Contract)

These resource keys are guaranteed to exist and can be referenced by any View:

| Key | Type | Description |
|-----|------|-------------|
| `WorkspaceTabStyle` | `Style(TabItem)` | Active/inactive tab with accent underline |
| `PanelBorderStyle` | `Style(Border)` | Card/panel border with brand border color |
| `AIAssistantPanelStyle` | `Style(Border)` | 320px expanded / 48px collapsed side panel |
| `SectionHeaderStyle` | `Style(TextBlock)` | Section heading typography |
| `BodyTextStyle` | `Style(TextBlock)` | Body text with brand font family |
| `CardSurfaceStyle` | `Style(Border)` | KPI card surface with card background |
| `KpiCardStyle` | `Style(Border)` | 200x120 KPI card with branded surface |
| `KpiValueStyle` | `Style(TextBlock)` | Large KPI value text |
| `KpiLabelStyle` | `Style(TextBlock)` | Small KPI label text |
| `ActionTileStyle` | `Style(Button)` | Branded quick action tile |
| `DashboardTitleStyle` | `Style(TextBlock)` | Dashboard section title |
| `EmptyStateContainerStyle` | `Style(Border)` | Empty state container with brand surface |
| `EmptyStateIconStyle` | `Style(FontIcon)` | Fluent UI icon for empty state |
| `EmptyStateTitleStyle` | `Style(TextBlock)` | Empty state title |
| `EmptyStateDescriptionStyle` | `Style(TextBlock)` | Empty state description |
| `BrandFontFamily` | `FontFamily` | Primary + fallback font family |
| `ShellBackgroundGradient` | `LinearGradientBrush` | Main shell background gradient |
| `DashboardBackgroundGradient` | `LinearGradientBrush` | Dashboard area gradient |
| `WorkspaceBackgroundGradient` | `LinearGradientBrush` | Workspace area gradient |

### Brand Asset Pack URIs (Contract)

| Asset | Pack URI |
|-------|----------|
| Logo Dark | `pack://application:,,,/Resources/Branding/LogoDark.png` |
| Logo Light | `pack://application:,,,/Resources/Branding/LogoLight.png` |
| Logo Monochrome | `pack://application:,,,/Resources/Branding/LogoMonochrome.png` |
| Wordmark | `pack://application:,,,/Resources/Branding/Wordmark.png` |
| Inter Font | `pack://application:,,,/Resources/Branding/Inter/#Inter` |