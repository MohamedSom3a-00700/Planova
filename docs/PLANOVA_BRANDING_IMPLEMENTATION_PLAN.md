# Planova Branding Implementation Plan

**Status**: Mandatory Visual Identity Standard

**Date**: 2026-06-01

**Source of Truth**:

- [docs/03-BRAND_GUIDELINES.md](./03-BRAND_GUIDELINES.md)
- [docs/11-UI_UX_DESIGN_SYSTEM.md](./11-UI_UX_DESIGN_SYSTEM.md)
- [docs/PHASE_0_IMPLEMENTATION_PLAN.md](./PHASE_0_IMPLEMENTATION_PLAN.md)
- [Planova Branding Master](./Planova%20Branding%20Master)

## Purpose

This document defines the required visual identity implementation for Planova.
It is not a suggestion document. It is the exact branding pass that the product
shell must follow before later studios and feature modules are considered
visually complete.

The goal is to make the product look and feel like the reference branding
master, not merely “acceptable.” Every shell surface, navigation element, card,
panel, icon, and theme state must support the same identity language.

## Core Requirement

Planova must read as a branded engineering intelligence platform from the first
screen. If a new user opens the app and cannot immediately recognize the brand,
the branding pass is not complete.

## Non-Negotiables

1. The shell must not look like a generic WPF starter application.
2. The logo/wordmark system must be visible and consistent.
3. Navigation icons must use Fluent UI Icons, not ad hoc image files.
4. The visual language must match the branding master in tone and density.
5. Dark and light themes must feel like the same product family.
6. The dashboard must be a command center, not an empty placeholder.
7. The assistant panel must be styled as a persistent product element.
8. Spacing, hierarchy, and panel treatment must be deliberate and uniform.
9. All future modules must inherit this shell language without redesign.
10. No “temporary” generic styling is allowed where brand styling is required.

## Visual Target

The reference image shows a product with:

- a stronger branded header
- polished logo treatment
- restrained but deliberate iconography
- dense but balanced shell layout
- proper dashboard hierarchy
- visible assistant/AI presence
- clean cards and workspace chrome
- light and dark theme consistency

The implemented product must move toward that exact feeling.

## In Scope

- App logo and wordmark treatment
- Shell header composition
- Navigation rail identity
- Fluent UI icon system
- Dashboard composition and card styling
- Quick action styling
- Workspace tab styling
- AI assistant panel styling
- Theme tokens and palette fidelity
- Surface, border, spacing, and elevation rules
- Empty states and placeholder screens
- Hover, selected, pressed, and focus states
- Brand-safe typography and alignment

## Out of Scope

- New feature development
- Domain logic changes
- Workflow automation
- AI provider changes
- Database work
- Business module design

## Required Brand Assets

The following assets must be treated as product assets, not reference-only
images:

- logo variants
- logo light variant
- logo dark variant
- monochrome logo
- brand wordmark
- any supporting brand marks stored in the branding master folder

Asset rules:

- Logo assets are allowed to be raster or vector depending on quality.
- The logo must remain crisp at header size and taskbar thumbnail size.
- The logo must never be stretched, cropped, blurred, or over-resized.
- The logo must have a defined light and dark presentation mode if needed.

## Required Icon System

Official icon source:

- Fluent UI Icons

Rules:

- Use Fluent UI icons for navigation rail entries.
- Use Fluent UI icons for shell actions and quick actions.
- Use Fluent UI icons for toolbar buttons and module placeholders.
- Use Fluent UI icons consistently for labels, buttons, and commands where the
  reference implies an icon-plus-text treatment.
- Do not mix random bitmap icons with Fluent UI icons on the same surface.
- Do not use image files for icons when a Fluent UI icon exists.
- Icons must be monochrome or theme-aware wherever feasible.
- Icons must retain legibility at compact rail size and larger panel size.
- Review every icon against the adjacent word or action so it matches
  semantically.

## Fluent UI Styling Requirements

The implementation must visibly carry Fluent UI design language, not only the
package dependency.

Required Fluent UI cues:

- clean surface hierarchy
- disciplined spacing rhythm
- restrained radii
- subtle elevation
- modern shell chrome
- control styling that feels native to the brand
- consistent typography and icon behavior

Do not consider the implementation Fluent UI-compliant if it merely references
the library but still looks like a stock WPF surface.

## Typography Rules

Typography must be consistent with the product reference:

- use a clean Microsoft Fluent-aligned system font stack
- reserve large bold text for dashboard titles and section headers only
- use tighter, smaller labels for navigation and shell chrome
- do not use oversized decorative type
- do not scale fonts arbitrarily with viewport width
- keep letter spacing at zero
- ensure text fits inside its container on standard desktop resolutions

## Layout Rules

### Shell Structure

The shell must keep the reference composition:

- title bar/header row
- logo in the same row as tabs and window controls
- left navigation rail
- main workspace area
- right-side assistant panel if active
- bottom status area only if it adds value

The shell must not become a bloated ribbon UI.

### Top Bar Composition

The top bar must follow the reference intent:

- logo and wordmark must sit in the same top row as the tabs
- language and theme controls must sit next to the window controls in the
  title bar
- icons should be used for language/theme controls when the control meaning is
  icon-friendly
- the top bar must read as one integrated chrome area, not separate strips
- tabs must not be visually detached from the header row

### Background Treatment

The background must not be a flat solid field where the reference uses depth.

Requirements:

- use a subtle gradient or layered surface treatment where the reference calls
  for depth
- avoid dead-flat monochrome shell backgrounds
- preserve legibility and contrast
- keep the treatment restrained and product-like

### Navigation Rail

The rail must:

- use Fluent UI icons
- present labels with icons clearly
- treat every icon-label pairing as semantically matched
- support active/hover/focus states
- remain collapsible
- keep item spacing deliberate
- avoid oversized text-only items

### Workspace

The workspace must:

- use a tabbed, multi-view model
- keep content dense and organized
- avoid floating-card clutter
- preserve scanning and comparison ability
- support large-screen layouts naturally

### Dashboard

The dashboard must:

- feel like a command center
- show key metrics as cards
- show quick actions as branded controls
- show recent activity in a readable list
- keep plenty of usable workspace
- avoid empty decorative space where data can live
- include an AI assistant card or panel in the dashboard composition where the
  reference shows assistant presence
- keep assistant presence as a first-class product element, not a hidden one

### AI Assistant Panel

The assistant panel must:

- remain visible as a product feature, not a novelty
- be styled as a persistent side surface
- support collapsible behavior
- align visually with the rest of the shell
- reflect the same visual identity as the dashboard assistant card where both
  are present

## Required Surfaces

### Header

The header must include:

- Planova logo/wordmark treatment
- consistent top-level shell chrome
- clean spacing and alignment
- branded visual balance

### Navigation

The navigation must include:

- Dashboard
- Projects
- Clients
- Profile
- Reports
- BOQ
- WBS
- Scheduling
- Claims
- Settings
- Contracts

Each item must have a matching Fluent UI icon.

### Dashboard Cards

Cards must:

- use the approved surface tokens
- have consistent radius and padding
- support KPI hierarchy
- remain legible in dark and light themes

### Quick Actions

Quick actions must:

- use Fluent UI icons
- look like product actions, not generic buttons
- maintain a consistent tile size and spacing

### Tabs

Tabs must:

- be part of the top bar/header composition where the reference does that
- look integrated with the shell
- keep active tab treatment clear but restrained
- avoid looking like default control chrome
- use icons where the tab meaning benefits from an icon

## Theme Requirements

### Dark Theme

The dark theme must:

- match the reference dark presentation
- preserve contrast without harsh glare
- use refined surfaces and borders
- avoid washed-out or muddy colors

### Light Theme

The light theme must:

- remain clearly part of the same product family
- preserve brand accent identity
- keep cards, borders, and surfaces deliberate
- avoid generic default white-window look

## Visual States

All branded controls must define:

- normal state
- hover state
- pressed state
- selected/active state
- disabled state
- keyboard focus state

These states must be consistent across:

- navigation items
- action buttons
- quick actions
- tabs
- header controls

## Implementation Order

### 1. Brand Token System

Create the visual foundation used everywhere else.

Deliverables:

- palette tokens
- surface tokens
- border tokens
- accent tokens
- spacing scale
- icon sizing guidance
- radius and elevation treatment

### 2. Logo and Wordmark System

Apply brand identity to the top-level shell.

Deliverables:

- header logo placement
- wordmark sizing
- theme-aware logo selection
- consistent shell entrance identity

### 3. Navigation Identity Pass

Convert navigation from plain text into branded shell navigation.

Deliverables:

- Fluent UI icons
- aligned labels
- active state styling
- hover/focus feedback
- compact but readable rail behavior

### 4. Dashboard Identity Pass

Turn the dashboard into a branded command center.

Deliverables:

- branded KPI cards
- styled quick actions
- improved section hierarchy
- consistent empty/recent activity presentation

### 5. Workspace and Panel Identity Pass

Extend the same language to tabs and assistant surfaces.

Deliverables:

- branded tabs
- assistant panel styling
- consistent borders and spacing
- aligned chrome across surfaces

### 6. Theme Fidelity Pass

Refine the light and dark themes until they match the brand target.

Deliverables:

- dark theme polish
- light theme polish
- contrast tuning
- surface harmony review

### 7. Final Reference Review

Compare the implementation directly against the reference image set.

Deliverables:

- screenshot comparison
- icon consistency review
- spacing review
- logo presentation review
- panel and card review

## Detailed Acceptance Criteria

The branding pass is only complete when all of the following are true:

- The shell clearly reads as Planova without explanation.
- The logo/wordmark is visible and stable in the header.
- The navigation rail uses Fluent UI icons consistently.
- The dashboard resembles the branding reference in structure and tone.
- The theme system produces branded dark and light experiences.
- The assistant panel feels like a native product surface.
- The shell is visually coherent across all major areas.
- The style is suitable for later studios without rework.

## Definition of Done

- The product has a usable brand token system.
- The logo and wordmark are integrated into the shell.
- Fluent UI icons are applied to navigation and action surfaces.
- Dashboard visuals align with the reference direction.
- The shell composition feels intentional, not default.
- The branding foundation is ready for all subsequent phases.

## Do Not Accept

The following are not acceptable outcomes:

- plain text rail items with no icons
- generic default WPF styling
- unbranded header or title bar
- mismatched icon style
- cards that look like stock controls
- dashboard surfaces that feel empty
- AI panel treated as an afterthought
- light and dark themes that feel like unrelated products
- any visual treatment that reduces Planova’s identity clarity

## Next Step

Implement the branding pass on top of the existing functional shell before
moving to further product expansion. The visual language must be locked in
before later modules inherit the shell.
