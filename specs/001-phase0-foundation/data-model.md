# Data Model: Planova Phase 0 Foundation

## Entity: UserPreferences

Represents user-customizable application settings that persist across restarts.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | integer | PK, auto-increment | Unique identifier |
| ThemePreference | string | "Dark" or "Light" | Selected theme; defaults to Dark |
| LanguagePreference | string | "en" or "ar" | Selected UI language; defaults to en |
| WindowX | integer | nullable | Saved window left position |
| WindowY | integer | nullable | Saved window top position |
| WindowWidth | integer | nullable, > 0 | Saved window width |
| WindowHeight | integer | nullable, > 0 | Saved window height |
| WindowMaximized | boolean | default false | Whether window was maximized on close |
| CreatedAt | datetime | auto-set | Record creation timestamp |
| UpdatedAt | datetime | auto-set on change | Record last update timestamp |

**Identity**: Single row table (singleton pattern). Application reads preferences on startup and writes on changes.

**State transitions**: Preferences are loaded at startup → modified at runtime by user actions → saved on change or application exit.

## Entity: SchemaVersion

Tracks database schema version for migration management.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | integer | PK, auto-increment | Unique identifier |
| Version | string | unique, not null | Semantic version or migration identifier |
| AppliedAt | datetime | auto-set, not null | When this schema version was applied |
| Description | string | nullable | Human-readable description of the migration |

**Validation**: Schema version must be applied in order. Duplicate versions are rejected.

## Relationships

- `UserPreferences` is standalone (no foreign keys in Phase 0)
- `SchemaVersion` is standalone (tracks migration history only)
- Future entities will reference the foundation schema via new migrations

## Naming Conventions

- Tables: PascalCase singular (e.g., `UserPreferences`, `SchemaVersion`)
- Columns: PascalCase (C# convention matching EF Core defaults)
- Primary keys: `Id` on all entities
- Timestamps: `CreatedAt`, `UpdatedAt`
