# Quickstart: Planova Phase 0 Foundation

## Prerequisites

- Windows 10 or later
- .NET 8 SDK
- Visual Studio 2022 (recommended) or JetBrains Rider

## Getting Started

1. **Clone the repository**
   ```bash
   git clone <repo-url>
   cd Planova
   ```

2. **Restore and build**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run --project Planova.UI
   ```

## What to Expect

On first launch:
- The application creates a SQLite database in the app data directory
- Dark theme is applied by default
- UI language is set to English
- The shell displays with:
  - Collapsible navigation rail on the left
  - Multi-tab workspace in the center
  - Status bar at the bottom

## Verifying Phase 0 Completion

| Check | How to Verify |
|-------|---------------|
| Shell launches | Run the app; window appears within 5 seconds |
| Navigation works | Click a nav rail item; a tab opens in the workspace |
| Theme switching | Switch to light theme; all UI updates immediately |
| Language switching | Switch to Arabic; labels change and layout goes RTL |
| Settings persist | Change theme/language, restart; preferences are restored |
| Database initializes | Delete database file, restart; file is recreated with schema |
| Logging works | Check app logs directory for structured log output |

## Architecture Overview

```text
Planova.UI          → WPF shell, views, view models
Planova.Application → Application services and use case interfaces
Planova.Domain      → Domain entities (framework-free)
Planova.Infrastructure → Logging, configuration
Planova.Persistence → EF Core, SQLite, migrations
Planova.Localization → English and Arabic resource files
Planova.Shared      → Cross-layer abstractions
```

## Key Files

- `specs/001-phase0-foundation/spec.md` — Feature specification
- `specs/001-phase0-foundation/plan.md` — Implementation plan
- `specs/001-phase0-foundation/research.md` — Technology decisions
- `specs/001-phase0-foundation/data-model.md` — Entity definitions
- `specs/001-phase0-foundation/contracts/` — Layer interface contracts
