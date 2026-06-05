# Quickstart: Branding Visual Identity

**Feature**: `003-branding-visual-identity`
**Date**: 2026-06-01

## Prerequisites

- .NET 8 SDK
- WPF-supported Windows (10+)
- IDE: Visual Studio 2022 or Rider
- NuGet: Wpf.Ui package (to be added, replaces FluentWPF 0.10.2)

## Setup

### 1. Add Wpf.Ui Package (replace FluentWPF)

```powershell
# Remove unused FluentWPF
dotnet remove Planova.UI/Planova.UI.csproj package FluentWPF

# Add Wpf.Ui
dotnet add Planova.UI/Planova.UI.csproj package Wpf.Ui
```

### 2. Add Inter Font Resources

Place `Inter-Variable.ttf` and `Inter-Italic-Variable.ttf` in:
```
Planova.UI/Resources/Branding/Inter/
```

Set build action to `Resource` in the `.csproj`:
```xml
<ItemGroup>
  <Resource Include="Resources\Branding\Inter\Inter-Variable.ttf" />
  <Resource Include="Resources\Branding\Inter\Inter-Italic-Variable.ttf" />
</ItemGroup>
```

### 3. Add Brand Logo Assets

Ensure these files exist in `Planova.UI/Resources/Branding/`:
- `LogoDark.png`
- `LogoLight.png`
- `LogoMonochrome.png` (add from `Planova Branding Master/Logo-Monochrome.png`)
- `Wordmark.png`

Set build action to `Resource`.

### 4. Create New XAML Resource Files

Create the following new files under `Planova.UI/Styles/`:

| File | Purpose |
|------|---------|
| `HighContrastFallback.xaml` | Maps all brand tokens to `SystemColors` equivalents |
| `Gradients.xaml` | Adaptive gradient brushes per surface area |

### 5. Update App.xaml Merge Order

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Styles/ThemeTokens.xaml"/>
            <ResourceDictionary Source="Styles/BrandStyles.xaml"/>
            <ResourceDictionary Source="Styles/BrandSpacing.xaml"/>
            <ResourceDictionary Source="Styles/Gradients.xaml"/>
            <ResourceDictionary Source="Styles/BrandColors.xaml"/>
            <!-- Dynamic: DarkTheme.xaml, LightTheme.xaml, or HighContrastFallback.xaml -->
            <ResourceDictionary Source="Styles/DarkTheme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Key Implementation Steps

### Step 1: Token Alignment

Update `BrandColors.xaml` and `ThemeTokens.xaml` to use the authoritative palette:
- Dark: `#0D1117`, `#161B22`, `#1F2937`, `#2A3441`, `#00BFFF`, `#0078D4`
- Light: `#F8F9FB`, `#FFFFFF`, `#FFFFFF`, `#D6DCE5`, `#0078D4`, `#00BFFF`

### Step 2: Icon System Migration

Replace `NavIconConverter` (Segoe MDL2) with `Wpf.Ui.SymbolRegular`-based icon mapping. Each `NavigationItemViewModel` gets an `Icon` property of type `SymbolRegular`.

### Step 3: Navigation Expansion

Register 6 additional navigation targets in `ShellViewModel.RegisterNavigationTargets()`:
- Activity Studio, Resource Studio, Cost Studio, Primavera Studio, Schedule Compare, Delay Analysis, Chronology, Correspondence, Knowledge Base, Analytics, Integration Hub

Placeholder targets render the branded empty state.

### Step 4: Adaptive Gradients

Create `Gradients.xaml` with `ShellBackgroundGradient`, `DashboardBackgroundGradient`, and `WorkspaceBackgroundGradient` keyed resources. Apply via `DynamicResource` to shell elements.

### Step 5: Window Title Bar Branding

Replace `ShellView` title bar with `Wpf.Ui.TitleBar` control. Add logo image to `TitleBar.Header` or `TitleBar.Icon`.

### Step 6: High-Contrast Fallback

Implement `HighContrastDetector` service. When `SystemParameters.HighContrast` is `true`, swap theme dictionary to `HighContrastFallback.xaml`, remove `BrandColors.xaml` and `Gradients.xaml` from merged dictionaries.

### Step 7: AI Assistant Panel

Create `AssistantPanelView.xaml` as a collapsible panel (48px collapsed indicator → 320px expanded). Apply `AIAssistantPanelStyle`. Register in `INavigationService` or host in `ShellView` as persistent side panel.

### Step 8: Font Family Update

Define `BrandFontFamily` in `ThemeTokens.xaml`:
```xml
<FontFamily x:Key="BrandFontFamily">Segoe UI Variable, pack://application:,,,/Resources/Branding/Inter/#Inter</FontFamily>
```

Apply to all text styles via `FontFamily="{DynamicResource BrandFontFamily}"`.

## Verification Checklist

- [ ] All 17 navigation items appear with Fluent UI icons
- [ ] Dark theme colors match authoritative palette (`#0D1117`, `#161B22`, etc.)
- [ ] Light theme colors match authoritative palette (`#F8F9FB`, `#FFFFFF`, etc.)
- [ ] Logo appears in window title bar and shell header
- [ ] Theme switcher toggles dark/light with all tokens updating
- [ ] OS high-contrast mode suppresses all brand colors
- [ ] Adaptive gradient backgrounds render on shell, dashboard, workspace
- [ ] Inter font loads and renders correctly on systems without Segoe UI Variable
- [ ] RTL layout: navigation chrome mirrors, logo stays left-anchored
- [ ] AI assistant panel shows collapsed indicator and expands on click
- [ ] Empty states use branded styling with Fluent UI icons
- [ ] Wordmark collapses below 640px window width
- [ ] All text meets WCAG AA contrast ratios (4.5:1 normal, 3:1 large)