# Research: Planova Phase 0 Foundation

## Technology Decisions

### Application Host
- **Decision**: Microsoft.Extensions.Hosting
- **Rationale**: Provides standardized dependency injection, configuration, and application lifecycle management for desktop applications. Already mandated by Planova Constitution.
- **Alternatives considered**: Custom bootstrapper (rejected: reinvents what Hosting provides)

### UI Framework
- **Decision**: WPF + Fluent UI WPF + CommunityToolkit.Mvvm
- **Rationale**: Constitution mandates WPF for desktop UI, Fluent UI WPF for modern styling, and CommunityToolkit.Mvvm for MVVM infrastructure.
- **Alternatives considered**: WinForms (rejected: no modern binding/MVVM support), MAUI (rejected: not mature enough for complex desktop), Avalonia (rejected: not constitution-approved)

### Persistence
- **Decision**: SQLite + EF Core with Code First Migrations
- **Rationale**: Constitution mandates SQLite as primary database with EF Core ORM. Code First enables versioned schema evolution aligned with domain entities.
- **Alternatives considered**: Dapper (rejected: constitution mandates EF Core), SQL Server LocalDB (rejected: heavier dependency for single-user desktop)

### Logging
- **Decision**: Serilog
- **Rationale**: Constitution-mandated structured logging framework. Provides rolling file sink, console sink, and startup bootstrapping before host initialization.
- **Alternatives considered**: NLog (not constitution-approved), Microsoft.Extensions.Logging abstractions (used as facade, Serilog as provider)

### Localization
- **Decision**: .NET resource files (.resx) with custom runtime switching service
- **Rationale**: Standard .NET localization approach. Resource files provide compile-time safety and tooling support. Custom service enables runtime culture switching and RTL layout change.
- **Alternatives considered**: Database-driven localization (over-engineered for Phase 0), JSON-based resources (no built-in tooling)

### Navigation
- **Decision**: Custom Navigation Rail + Multi-Tab Workspace using WPF content switching
- **Rationale**: Constitution mandates Navigation Rail + Multi-Tab Workspace layout. Custom implementation gives full control over UX while keeping dependencies minimal.
- **Alternatives considered**: Prism regions (rejected: adds framework dependency without significant benefit for the shell pattern)

## Architecture Patterns

### Clean Architecture Layer Rules
- UI → references Application (interfaces) and Domain (entities)
- Application → references Domain only
- Domain → no dependencies
- Infrastructure → implements Application interfaces
- Persistence → implements Application interfaces, references Domain entities
- Localization → referenced by UI and Application
- Shared → no dependencies, referenced by all layers for common abstractions

### Error Handling Strategy
- All edge cases: show user-friendly dialog, log full diagnostic details, allow retry or graceful exit
- Unhandled exceptions: captured by global handler, logged with context, application may attempt graceful restart
- Startup failures: logged before UI host initializes

### Logging Scope
- Info level: startup events, settings load, database initialization, language/theme switches
- Error level: all exceptions and failures
- Retention: minimum 7 days or 100 MB (rolling file)

## Risk Mitigations

### Foundation coupling risk
- Keep Shared abstractions thin
- Push implementation details into Infrastructure/Persistence
- Use interface-based dependency injection

### Localization retrofit risk
- Make localization part of shell from the start
- Every shell string must be resource-backed

### Theme hardcoding risk
- Centralize theme tokens in resource dictionaries
- No inline color values in views

### Premature schema risk
- Create only foundation schema in Phase 0
- Defer feature entities to later phases
