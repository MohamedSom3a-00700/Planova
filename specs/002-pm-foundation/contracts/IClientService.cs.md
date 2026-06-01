# Contract: IClientService

**Layer**: Application

**Namespace**: `Planova.Application.Services`

**Purpose**: Defines the use cases for managing clients.

## Methods

```csharp
Task<IEnumerable<ClientSummaryDto>> GetAllAsync(CancellationToken ct = default);
Task<ClientDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
Task<ClientDetailDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default);
Task<ClientDetailDto> UpdateAsync(int id, UpdateClientDto dto, CancellationToken ct = default);
Task DeleteAsync(int id, CancellationToken ct = default);
Task<IEnumerable<ClientSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
```

## DTOs

### ClientSummaryDto

```csharp
public record ClientSummaryDto(
    int Id, string Code, string Name, string? ContactEmail,
    int ProjectCount, DateTime UpdatedAt
);
```

### ClientDetailDto

```csharp
public record ClientDetailDto(
    int Id, string Code, string Name,
    string? ContactEmail, string? ContactPhone,
    string? OrganizationDetails, string? Notes,
    List<ProjectSummaryDto> Projects,
    List<ContractSummaryDto> Contracts,
    DateTime CreatedAt, DateTime UpdatedAt
);
```

### CreateClientDto

```csharp
public record CreateClientDto(
    string Code, string Name,
    string? ContactEmail, string? ContactPhone,
    string? OrganizationDetails, string? Notes
);
```

### UpdateClientDto

```csharp
public record UpdateClientDto(
    string Code, string Name,
    string? ContactEmail, string? ContactPhone,
    string? OrganizationDetails, string? Notes
);
```
