# Contract: IContractService

**Layer**: Application

**Namespace**: `Planova.Application.Services`

**Purpose**: Defines the use cases for managing contracts.

## Methods

```csharp
Task<IEnumerable<ContractSummaryDto>> GetAllAsync(CancellationToken ct = default);
Task<ContractDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
Task<ContractDetailDto> CreateAsync(CreateContractDto dto, CancellationToken ct = default);
Task<ContractDetailDto> UpdateAsync(int id, UpdateContractDto dto, CancellationToken ct = default);
Task DeleteAsync(int id, CancellationToken ct = default);
Task<IEnumerable<ContractSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
Task<IEnumerable<ContractSummaryDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
Task<IEnumerable<ContractSummaryDto>> GetByClientAsync(int clientId, CancellationToken ct = default);
```

## DTOs

### ContractSummaryDto

```csharp
public record ContractSummaryDto(
    int Id, string Number, string Title,
    decimal? Value, string? Currency, string? Status,
    string? ProjectName, string? ClientName,
    DateTime UpdatedAt
);
```

### ContractDetailDto

```csharp
public record ContractDetailDto(
    int Id, string Number, string Title,
    decimal? Value, string? Currency,
    DateTime? AwardDate, DateTime? CommencementDate, DateTime? CompletionDate,
    string? Status, string? Notes,
    int ProjectId, string ProjectName,
    int ClientId, string ClientName,
    DateTime CreatedAt, DateTime UpdatedAt
);
```

### CreateContractDto / UpdateContractDto

```csharp
public record CreateContractDto(
    string Number, string Title, decimal? Value, string? Currency,
    DateTime? AwardDate, DateTime? CommencementDate, DateTime? CompletionDate,
    string? Status, string? Notes,
    int ProjectId, int ClientId
);

public record UpdateContractDto(
    string Number, string Title, decimal? Value, string? Currency,
    DateTime? AwardDate, DateTime? CommencementDate, DateTime? CompletionDate,
    string? Status, string? Notes,
    int ProjectId, int ClientId
);
```
