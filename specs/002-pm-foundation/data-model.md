# Data Model: Project Management Foundation

**Date**: 2026-06-01

## Entity Relationship Diagram (text)

```
Project ──1:N──> Contract <──N:1── Client
   |                                      ^
   |                                      |
   +──*──> Client (optional, via ClientId)
   
UserProfile (standalone — no relationships to business entities)
```

## Entity: Project

**Namespace**: `Planova.Domain.Entities`

**Table**: `Projects`

### Fields

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| Code | string | Required, max 50, unique index | Business identifier |
| Name | string | Required, max 200 | |
| Description | string? | Max 2000 | |
| Status | string | Required, max 30 | Must be a valid status per value object |
| StartDate | DateTime? | | |
| FinishDate | DateTime? | Must be >= StartDate if both set | |
| ClientId | int? | FK -> Clients.Id, nullable | Optional link |
| Currency | string? | Max 10 | e.g., USD, EUR |
| Location | string? | Max 200 | |
| Notes | string? | Max 4000 | |
| CreatedAt | DateTime | Default: datetime('now') | Auto-set |
| UpdatedAt | DateTime | Default: datetime('now') | Updated on save |

### Indexes

- `IX_Projects_Code` — unique on `Code`
- `IX_Projects_Status` — non-unique on `Status`
- `IX_Projects_ClientId` — non-unique on `ClientId`

### Relationships

- **Client**: Optional many-to-one (Project -> Client). A project may have zero or one client.
- **Contracts**: One-to-many (Project -> Contract). A project may have multiple contracts.

### Status Lifecycle

Valid transitions implemented via `ProjectStatus` value object:

```
Draft ──> Under Review ──> Approved ──> In Progress ──> Completed
                                          │
                                          v
                                       On Hold
                                         │
                                         v
                                     In Progress

Any status ──> Cancelled  (terminal)
Any status ──> Completed  (terminal, only from non-terminal states)
```

## Entity: Client

**Namespace**: `Planova.Domain.Entities`

**Table**: `Clients`

### Fields

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| Code | string | Required, max 50, unique index | |
| Name | string | Required, max 200, unique index | Unique within organization |
| ContactEmail | string? | Max 200 | |
| ContactPhone | string? | Max 50 | |
| OrganizationDetails | string? | Max 2000 | |
| Notes | string? | Max 4000 | |
| CreatedAt | DateTime | Default: datetime('now') | |
| UpdatedAt | DateTime | Default: datetime('now') | |

### Indexes

- `IX_Clients_Code` — unique on `Code`
- `IX_Clients_Name` — unique on `Name`

### Relationships

- **Projects**: One-to-many (Client -> Project). A client may have multiple projects.
- **Contracts**: One-to-many (Client -> Contract). A client may have multiple contracts.

## Entity: Contract

**Namespace**: `Planova.Domain.Entities`

**Table**: `Contracts`

### Fields

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| Number | string | Required, max 100, unique index | Business identifier |
| Title | string | Required, max 200 | |
| Value | decimal? | Precision 18,2; must be >= 0 | |
| Currency | string? | Max 10 | e.g., USD, EUR |
| AwardDate | DateTime? | | |
| CommencementDate | DateTime? | | |
| CompletionDate | DateTime? | Must be >= CommencementDate if both set | |
| Status | string? | Max 30 | e.g., Active, Completed, Terminated |
| ProjectId | int | Required, FK -> Projects.Id | |
| ClientId | int | Required, FK -> Clients.Id | |
| Notes | string? | Max 4000 | |
| CreatedAt | DateTime | Default: datetime('now') | |
| UpdatedAt | DateTime | Default: datetime('now') | |

### Indexes

- `IX_Contracts_Number` — unique on `Number`
- `IX_Contracts_ProjectId` — non-unique on `ProjectId`
- `IX_Contracts_ClientId` — non-unique on `ClientId`

### Relationships

- **Project**: Required many-to-one (Contract -> Project). A contract must belong to one project.
- **Client**: Required many-to-one (Contract -> Client). A contract must belong to one client.

## Entity: UserProfile (extension of existing UserPreferences)

**Namespace**: `Planova.Domain.Entities`

The existing `UserPreferences` entity will be extended (or a new `UserProfile` entity added) with additional fields:

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | Reuse existing |
| DisplayName | string | Max 100 | New field |
| RoleLabel | string? | Max 100 | New — UI label only, not security |
| OrganizationName | string? | Max 200 | New |
| ThemePreference | string | Default: "Dark" | Existing |
| LanguagePreference | string | Default: "en" | Existing |
| DefaultWorkspace | string? | Max 50 | New — e.g., "dashboard", "projects" |
| CreatedAt | DateTime | Existing | |
| UpdatedAt | DateTime | Existing | |

## Value Objects

### ProjectStatus

**Namespace**: `Planova.Domain.ValueObjects`

A fixed set of allowed status values with a transition map:

```csharp
public sealed class ProjectStatus
{
    public static readonly ProjectStatus Draft = new("Draft");
    public static readonly ProjectStatus UnderReview = new("Under Review");
    public static readonly ProjectStatus Approved = new("Approved");
    public static readonly ProjectStatus InProgress = new("In Progress");
    public static readonly ProjectStatus OnHold = new("On Hold");
    public static readonly ProjectStatus Completed = new("Completed");
    public static readonly ProjectStatus Cancelled = new("Cancelled");
    
    private static readonly Dictionary<ProjectStatus, HashSet<ProjectStatus>> Transitions = new()
    {
        [Draft] = { UnderReview, Cancelled },
        [UnderReview] = { Approved, Draft, Cancelled },
        [Approved] = { InProgress, Cancelled },
        [InProgress] = { OnHold, Completed, Cancelled },
        [OnHold] = { InProgress, Completed, Cancelled },
        [Completed] = { },
        [Cancelled] = { },
    };
    
    public string Value { get; }
    
    public bool CanTransitionTo(ProjectStatus target) => ...;
    public IReadOnlySet<ProjectStatus> AllowedNext() => ...;
}
```

### Currency

**Namespace**: `Planova.Domain.ValueObjects`

Simple value object wrapping a 3-letter ISO currency code string.

## Validation Rules Summary

| Entity | Field | Rule |
|--------|-------|------|
| Project | Code | Required, unique, max 50 chars |
| Project | Name | Required, max 200 chars |
| Project | Status | Must be a valid ProjectStatus value |
| Project | FinishDate | Must be >= StartDate (if both set) |
| Project | Status transition | Must follow defined lifecycle |
| Client | Code | Required, unique, max 50 chars |
| Client | Name | Required, unique, max 200 chars |
| Contract | Number | Required, unique, max 100 chars |
| Contract | Title | Required, max 200 chars |
| Contract | Value | Must be >= 0 |
| Contract | CompletionDate | Must be >= CommencementDate (if both set) |
| Contract | ProjectId | Required |
| Contract | ClientId | Required |
