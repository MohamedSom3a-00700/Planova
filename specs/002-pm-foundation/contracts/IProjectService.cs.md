# Contract: IProjectService

**Layer**: Application

**Namespace**: `Planova.Application.Services`

**Purpose**: Defines the use cases for managing projects — the contract between the UI layer and the application layer.

## Methods

### GetAllAsync

```csharp
Task<IEnumerable<ProjectSummaryDto>> GetAllAsync(CancellationToken ct = default)
```

Returns all projects as summary DTOs (Id, Code, Name, Status, ClientName, StartDate, FinishDate).

### GetByIdAsync

```csharp
Task<ProjectDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
```

Returns a single project with all fields including linked contracts summary.

### CreateAsync

```csharp
Task<ProjectDetailDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default)
```

Creates a new project. Throws `DuplicateEntityException` if Code already exists. Throws `ValidationException` if validation fails.

### UpdateAsync

```csharp
Task<ProjectDetailDto> UpdateAsync(int id, UpdateProjectDto dto, CancellationToken ct = default)
```

Updates an existing project. Throws `EntityNotFoundException` if not found. Throws `DuplicateEntityException` if Code change conflicts with existing. Returns updated project.

### DeleteAsync

```csharp
Task DeleteAsync(int id, CancellationToken ct = default)
```

Deletes a project. Throws `EntityNotFoundException` if not found. Throws `EntityInUseException` if project has linked contracts.

### ChangeStatusAsync

```csharp
Task<ProjectDetailDto> ChangeStatusAsync(int id, string newStatus, CancellationToken ct = default)
```

Transitions project to a new status. Throws `InvalidTransitionException` if the transition is not allowed by the lifecycle rules.

### SearchAsync

```csharp
Task<IEnumerable<ProjectSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
```

Searches projects by name or code (partial match, case-insensitive). Returns matching summaries.

### GetByStatusAsync

```csharp
Task<IEnumerable<ProjectSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default)
```

Filters projects by a specific status value.

### GetDashboardSummaryAsync

```csharp
Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
```

Returns aggregated dashboard data: counts by status, recent activity list, total counts.

## DTOs

### ProjectSummaryDto

```csharp
public record ProjectSummaryDto(
    int Id,
    string Code,
    string Name,
    string Status,
    string? ClientName,
    DateTime? StartDate,
    DateTime? FinishDate,
    DateTime UpdatedAt
);
```

### ProjectDetailDto

```csharp
public record ProjectDetailDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    string Status,
    DateTime? StartDate,
    DateTime? FinishDate,
    string? Currency,
    string? Location,
    string? Notes,
    int? ClientId,
    string? ClientName,
    List<ContractSummaryDto> Contracts,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string[] AllowedNextStatuses  // Dynamic list of valid transition targets
);
```

### CreateProjectDto

```csharp
public record CreateProjectDto(
    string Code,
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? FinishDate,
    string? Currency,
    string? Location,
    int? ClientId,
    string? Notes
);
```

### UpdateProjectDto

```csharp
public record UpdateProjectDto(
    string Code,
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? FinishDate,
    string? Currency,
    string? Location,
    int? ClientId,
    string? Notes
);
```
