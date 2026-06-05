# Data Model: Resource Studio

## Entity Relationship Diagram

```
Resource (base)
  │
  ├── ResourceRate (1:N) — rates with effective dates
  ├── CrewResource (1:N) — membership in crew templates
  ├── ResourceAssignment (1:N) — assignments to activities
  └── ResourceUsage (1:N) — daily usage records

Crew (independent aggregate)
  └── CrewResource (1:N) — resources composing the crew

ResourceAssignment (links Resource + Activity)
  └── ResourceUsage (1:N) — daily planned quantities
```

---

## Entities

### Resource

The core entity representing any resource (Labour, Equipment, Material, Subcontractor).

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `Code` | `string(20)` | Yes | Auto-generated per FR-004: `R-{TYPE}-{NNN}` |
| `Name` | `string(200)` | Yes | Display name |
| `ResourceType` | `ResourceType` enum | Yes | Labour, Equipment, Material, Subcontractor — discriminator |
| `Scope` | `ResourceScope` enum | Yes | Global or Project |
| `ProjectId` | `int?` | No | Null for Global scope; FK → Project |
| `Status` | `ResourceStatus` enum | Yes | Active, Inactive (soft delete) |
| `DefaultRate` | `decimal(18,4)` | Yes | Fallback rate when no rate record matches |
| `UnitOfMeasure` | `string(50)` | Yes | e.g., "hr", "day", "ea", "ton" |
| `MaxQuantity` | `decimal(18,4)?` | No | Available quantity for overallocation detection |
| `Currency` | `string(3)` | Yes | ISO 4217 currency code, default "USD" |
| `Description` | `string(500)?` | No | Optional description |
| `IsGlobal` | `bool` | Yes | Computed from Scope == Global |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |
| `CreatedBy` | `string(100)?` | No | |
| `ModifiedBy` | `string(100)?` | No | |

**Type-specific fields (all nullable, only relevant for the discriminator type):**

| Field | Type | Applies To | Notes |
|-------|------|------------|-------|
| `Trade` | `string(100)?` | Labour | e.g., Carpenter, Welder, Electrician |
| `SkillLevel` | `string(50)?` | Labour | e.g., Apprentice, Journeyman, Foreman |
| `EquipmentType` | `string(100)?` | Equipment | e.g., Crane, Excavator, Forklift |
| `Capacity` | `string(100)?` | Equipment | e.g., "5 ton", "50 ft boom" |
| `OperatingCost` | `decimal(18,4)?` | Equipment | Additional cost per hour of operation |
| `UnitPrice` | `decimal(18,4)?` | Material | Price per unit of measure |
| `WastagePercent` | `decimal(5,2)?` | Material | Expected wastage percentage |
| `Company` | `string(200)?` | Subcontractor | Subcontractor company name |
| `ContractValue` | `decimal(18,2)?` | Subcontractor | Total subcontract value |
| `ContactName` | `string(100)?` | Subcontractor | Contact person |
| `ContactPhone` | `string(50)?` | Subcontractor | |

**Navigation Properties:**
- `Rates: ICollection<ResourceRate>`
- `CrewMemberships: ICollection<CrewResource>`
- `Assignments: ICollection<ResourceAssignment>`
- `UsageRecords: ICollection<ResourceUsage>`

---

### ResourceRate

An effective-dated rate for a resource.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ResourceId` | `Guid` | Yes | FK → Resource |
| `EffectiveDate` | `DateTime` | Yes | Date this rate becomes active |
| `Rate` | `decimal(18,4)` | Yes | Rate value |
| `Currency` | `string(3)` | Yes | ISO 4217, per-rate currency (FR-006: mixed currencies allowed) |
| `UnitOfMeasure` | `string(50)` | Yes | e.g., "hr", "day", "ea" |
| `IsDefault` | `bool` | Yes | Whether this is the default rate |
| `Notes` | `string(500)?` | No | |
| `CreatedAt` | `DateTime` | Yes | |

**Unique Constraint:** `(ResourceId, EffectiveDate)` — no two rates can have the same effective date for the same resource (per clarifications).

**Index:** `(ResourceId, EffectiveDate DESC)` for efficient rate resolution.

---

### Crew

A reusable template grouping resources into a standard work team.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `Name` | `string(200)` | Yes | e.g., "Concrete Crew", "Steel Erection Crew" |
| `Description` | `string(500)?` | No | |
| `ProjectId` | `int?` | No | Null for project-agnostic templates |
| `Status` | `CrewStatus` enum | Yes | Draft, Active, Inactive |
| `Category` | `string(100)?` | No | For grouping/filtering crews |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Navigation Properties:**
- `Resources: ICollection<CrewResource>`

**Constraints:**
- A crew must have at least one `CrewResource` to be usable (FR-013: apply disabled when empty)

---

### CrewResource

Links a specific resource to a crew template with a quantity.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `CrewId` | `Guid` | Yes | FK → Crew |
| `ResourceId` | `Guid` | Yes | FK → Resource |
| `Quantity` | `decimal(18,4)` | Yes | Number of this resource in the crew |
| `IsLead` | `bool` | Yes | Whether this resource is the crew lead/foreman |
| `SortOrder` | `int` | Yes | Display ordering within crew |

**Unique Constraint:** `(CrewId, ResourceId)` — a resource can only be added once to a crew.

**Computed (display only):**
- `EffectiveRate`: resolved from `Resource.Rates` at time of display
- `LineTotal`: `Quantity × EffectiveRate`

---

### ResourceAssignment

Links a resource (or crew) to a Phase 5 activity.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `ProjectId` | `int` | Yes | FK → Project (denormalized for query performance) |
| `ActivityId` | `Guid` | Yes | FK → Activity (Phase 5) |
| `ResourceId` | `Guid` | Yes | FK → Resource |
| `CrewId` | `Guid?` | No | FK → Crew if assigned via crew template (null for direct assignments) |
| `Quantity` | `decimal(18,4)` | Yes | Quantity of resource |
| `Rate` | `decimal(18,4)` | Yes | Applied rate (snapshot at time of assignment) |
| `Currency` | `string(3)` | Yes | Currency of the rate at assignment time |
| `UnitOfMeasure` | `string(50)` | Yes | |
| `StartDate` | `DateTime?` | No | Start of assignment (null if not date-bound) |
| `EndDate` | `DateTime?` | No | End of assignment (null if not date-bound) |
| `TotalCost` | `decimal(18,2)` | Yes | Computed: `Quantity × Rate × DurationFactor` |
| `DurationDays` | `decimal(18,2)?` | No | Computed from start-end date difference for hourly resources |
| `Notes` | `string(500)?` | No | |
| `CreatedAt` | `DateTime` | Yes | |
| `UpdatedAt` | `DateTime` | Yes | |

**Navigation Properties:**
- `Resource: Resource`
- `Activity: Activity` (Phase 5 entity)
- `Crew: Crew?`
- `UsageRecords: ICollection<ResourceUsage>`

**Constraints:**
- If `CrewId` is set, the assignment was created from a crew template and the individual resources are tracked
- `TotalCost` is computed on save and stored (not computed on every read) for performance
- Activity deletion is blocked when assignments exist (per clarifications)

---

### ResourceUsage

Daily resource usage records for histogram computation.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes | Primary key |
| `AssignmentId` | `Guid` | Yes | FK → ResourceAssignment |
| `ResourceId` | `Guid` | Yes | FK → Resource (denormalized) |
| `Date` | `DateTime` | Yes | The date this usage applies to |
| `PlannedQuantity` | `decimal(18,4)` | Yes | Quantity planned for this date |
| `ActualQuantity` | `decimal(18,4)?` | No | Actual quantity (future use) |

**Index:** `(ResourceId, Date)` for histogram queries.
**Index:** `(Date, ResourceId)` for time-range filtered queries.

---

## Enums

### ResourceType

```csharp
public enum ResourceType
{
    Labour,
    Equipment,
    Material,
    Subcontractor
}
```

### ResourceScope

```csharp
public enum ResourceScope
{
    Global,   // Shared across all projects
    Project   // Specific to one project
}
```

### ResourceStatus

```csharp
public enum ResourceStatus
{
    Active,
    Inactive  // Soft delete — still referenced by existing assignments
}
```

### CrewStatus

```csharp
public enum CrewStatus
{
    Draft,
    Active,
    Inactive
}
```

### UnitOfMeasure

```csharp
public enum UnitOfMeasure
{
    Hour,   // hr
    Day,    // day
    Each,   // ea
    Ton,    // ton
    CubicMeter, // m³
    SquareMeter, // m²
    LinearMeter, // lm
    LumpSum,     // ls
    Week,   // wk
    Month   // mo
}
```

---

## Validation Rules

### Resource

| Rule | Condition | Error |
|------|-----------|-------|
| Code uniqueness | `Code` + `Scope` must be unique | "A resource with this code already exists in this scope" |
| Name duplicate warning | `Name` matches existing in same scope | Display warning, allow save (FR-005b) |
| Type-specific fields | Fields must match `ResourceType` | "Field X is required for ResourceType Y" |
| Scope consistency | If `Scope == Project`, `ProjectId` must be set | "Project-scoped resources must have a project" |
| Hard delete blocked | Resource referenced by CrewResource or ResourceAssignment | "Cannot delete: resource is referenced by N crew(s) and M assignment(s). Deactivate instead." |

### ResourceRate

| Rule | Condition | Error |
|------|-----------|-------|
| Duplicate effective date | `(ResourceId, EffectiveDate)` already exists | "A rate with this effective date already exists for this resource" |
| Future date allowed | `EffectiveDate > Today` | Allowed — becomes active automatically |
| Rate value | `Rate > 0` | "Rate must be greater than zero" |

### Crew

| Rule | Condition | Error |
|------|-----------|-------|
| Empty crew | `CrewResources.Count == 0` | "Cannot apply a crew with no resources" |
| Duplicate resource | Same resource added twice | "Resource already in this crew" |

### ResourceAssignment

| Rule | Condition | Error |
|------|-----------|-------|
| Quantity | `Quantity > 0` | "Quantity must be greater than zero" |
| Rate | `Rate >= 0` | "Rate cannot be negative" |
| Date range | If `StartDate` and `EndDate` set, `EndDate >= StartDate` | "End date must be on or after start date" |
| Activity deletion | Activity has assignments | "Remove all resource assignments before deleting this activity" |

### ResourceUsage

| Rule | Condition | Error |
|------|-----------|-------|
| Missing dates | Assignment has no `StartDate`/`EndDate` | Assignment excluded from histogram with indication |
| Negative quantity | `PlannedQuantity < 0` | "Planned quantity cannot be negative" |

---

## State Transitions

### Resource Status

```
Active ──deactivate──→ Inactive
Inactive ──reactivate──→ Active
```

- **Active**: Resource is visible in pickers and can be assigned to new activities
- **Inactive**: Resource hidden from pickers; existing assignments remain valid and maintain their computed rates
- Direct transition to `Inactive` is allowed regardless of assignments (preserves historical data)

### Crew Status

```
Draft ──activate──→ Active
Active ──deactivate──→ Inactive
Inactive ──reactivate──→ Active
Draft ──→ Inactive (direct)
```

- **Draft**: Crew cannot be applied to activities
- **Active**: Crew can be applied to activities
- **Inactive**: Crew hidden from pickers; existing applications remain in place

---

## Index Strategy

| Table | Index | Type | Purpose |
|-------|-------|------|---------|
| Resource | `(Scope, ProjectId)` | Non-clustered | Library listing filtering |
| Resource | `(ResourceType, Scope)` | Non-clustered | Type filtering |
| Resource | `(Code)` | Unique non-clustered | Code lookup |
| Resource | `(Name)` | Non-clustered | Name search |
| ResourceRate | `(ResourceId, EffectiveDate DESC)` | Non-clustered | Rate resolution query |
| ResourceRate | `(ResourceId, EffectiveDate)` | Unique | Duplicate prevention |
| CrewResource | `(CrewId, ResourceId)` | Unique | Prevent duplicates |
| ResourceAssignment | `(ActivityId)` | Non-clustered | Activity assignment lookup |
| ResourceAssignment | `(ProjectId, ResourceId)` | Non-clustered | Project-level queries |
| ResourceUsage | `(ResourceId, Date)` | Non-clustered | Histogram queries |
| ResourceUsage | `(Date, ResourceId)` | Non-clustered | Time-range filtered histogram |
