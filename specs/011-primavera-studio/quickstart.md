# Quickstart: Primavera Studio

**Branch**: `011-primavera-studio` | **Date**: 2026-06-12 | **Plan**: [plan.md](plan.md)

## Module Overview

`Planova.Primavera` is a new class library module following the same pattern as `Planova.Cost`, `Planova.Resource`, and `Planova.Reporting`.

## Project Setup

```shell
# Create the module project
dotnet new classlib -n Planova.Primavera -o Planova.Primavera
dotnet new xunit -n Planova.Primavera.Tests -o tests/Planova.Primavera.Tests

# Add to solution
dotnet sln add Planova.Primavera/Planova.Primavera.csproj
dotnet sln add tests/Planova.Primavera.Tests/Planova.Primavera.Tests.csproj

# Add project references
dotnet add Planova.Primavera/Planova.Primavera.csproj reference Planova.Shared/Planova.Shared.csproj
dotnet add tests/Planova.Primavera.Tests/Planova.Primavera.Tests.csproj reference Planova.Primavera/Planova.Primavera.csproj
```

## Key Dependencies

- .NET 8 / C# 12
- CommunityToolkit.Mvvm
- EF Core 8 + SQLite
- Fluent UI WPF
- Serilog

## Directory Layout

```
Planova.Primavera/
├── Domain/           # Entities, enums, interfaces, constants
├── Application/      # Services, DTOs, parsers, writers, mappings
├── Background/       # Hosted services
├── Extensions/       # DI registration
└── Planova.Primavera.csproj
```

## Implementation Order

1. **Foundation** — Module project, domain entities, enums, interfaces
2. **Persistence** — EF Core configs, DbContext extensions, repositories
3. **XER Import/Export** — Parser, writer, preview, staged commit
4. **Validation & Repair** — Validation rules, repair suggestions
5. **Workspace UI** — ViewModels, Views, navigation wiring
6. **Polish** — Localization, tests, benchmarks

## Cross-Studio Consumption

Other studios inject `IPrimaveraWorkspaceService?` (nullable) for Primavera data. See [contracts/cross-studio-interfaces.md](contracts/cross-studio-interfaces.md) for full contract details.

## Key References

- [Specification](spec.md)
- [Implementation Plan](plan.md)
- [Data Model](data-model.md)
- [Cross-Studio Contracts](contracts/cross-studio-interfaces.md)
- [Phase 9 Implementation Plan](../../docs/PHASE_9_IMPLEMENTATION_PLAN.md)
