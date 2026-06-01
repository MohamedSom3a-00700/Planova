# Application Contracts: Project Management Foundation

These contracts define the interfaces between the UI (Presentation) layer and the Application layer, following Clean Architecture dependency rules.

| Contract | File | Description |
|----------|------|-------------|
| IProjectService | [IProjectService.cs.md](./IProjectService.cs.md) | Project CRUD, status transitions, search, dashboard aggregation |
| IClientService | [IClientService.cs.md](./IClientService.cs.md) | Client CRUD, search, linked entity queries |
| IContractService | [IContractService.cs.md](./IContractService.cs.md) | Contract CRUD, search, filtered by project/client |

## Shared DTOs

These DTOs are shared across multiple contracts:

| DTO | Used By | Fields |
|-----|---------|--------|
| ProjectSummaryDto | IProjectService.GetAll, Search, GetByStatus; IClientService.GetById (Projects list) | Id, Code, Name, Status, ClientName, StartDate, FinishDate, UpdatedAt |
| ContractSummaryDto | IContractService.GetAll, Search, GetByProject, GetByClient; IProjectService.GetById (Contracts list) | Id, Number, Title, Value, Currency, Status, ProjectName, ClientName, UpdatedAt |

## Exception Contracts

| Exception | Thrown By | When |
|-----------|-----------|------|
| DuplicateEntityException | Create, Update (Code/Number conflict) | Unique constraint violation |
| EntityNotFoundException | GetById, Update, Delete (entity missing) | Referenced entity does not exist |
| EntityInUseException | Delete (linked children exist) | Entity cannot be deleted due to dependencies |
| InvalidTransitionException | ChangeStatus (invalid status move) | Status transition not in allowed map |
| ValidationException | Create, Update (field validation failure) | Required fields missing, date ordering invalid, etc. |
