# Domain Interfaces — Resource Studio

## Repository Contracts

### IResourceRepository

```csharp
public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Resource>> GetByProjectAsync(int projectId, ResourceScope? scope = null,
        ResourceType? type = null, CancellationToken ct = default);
    Task<List<Resource>> SearchAsync(string query, ResourceType? type = null,
        ResourceScope? scope = null, int? projectId = null, CancellationToken ct = default);
    Task<List<Resource>> GetByTypeAsync(ResourceType type, ResourceScope? scope = null,
        int? projectId = null, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, ResourceScope scope, int? projectId = null,
        CancellationToken ct = default);
    Task<string> GenerateNextCodeAsync(ResourceType type, CancellationToken ct = default);
    Task<bool> HasDuplicateNameAsync(string name, ResourceScope scope, int? projectId = null,
        Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Resource resource, CancellationToken ct = default);
    Task UpdateAsync(Resource resource, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetCountAsync(ResourceType? type = null, ResourceScope? scope = null,
        CancellationToken ct = default);
}
```

### IResourceRateRepository

```csharp
public interface IResourceRateRepository
{
    Task<ResourceRate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceRate>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default);
    Task<ResourceRate?> GetEffectiveRateAsync(Guid resourceId, DateTime date,
        CancellationToken ct = default);
    Task<bool> HasDuplicateEffectiveDateAsync(Guid resourceId, DateTime effectiveDate,
        Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(ResourceRate rate, CancellationToken ct = default);
    Task UpdateAsync(ResourceRate rate, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task BulkUpdateAsync(List<(Guid ResourceId, decimal NewRate)> rateUpdates,
        DateTime effectiveDate, CancellationToken ct = default);
}
```

### ICrewRepository

```csharp
public interface ICrewRepository
{
    Task<Crew?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Crew>> GetAllAsync(int? projectId = null, CrewStatus? status = null,
        CancellationToken ct = default);
    Task<List<Crew>> SearchAsync(string query, int? projectId = null,
        CancellationToken ct = default);
    Task AddAsync(Crew crew, CancellationToken ct = default);
    Task UpdateAsync(Crew crew, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### ICrewResourceRepository

```csharp
public interface ICrewResourceRepository
{
    Task<List<CrewResource>> GetByCrewAsync(Guid crewId, CancellationToken ct = default);
    Task<CrewResource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CrewResource crewResource, CancellationToken ct = default);
    Task UpdateAsync(CrewResource crewResource, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteByCrewAsync(Guid crewId, CancellationToken ct = default);
}
```

### IResourceAssignmentRepository

```csharp
public interface IResourceAssignmentRepository
{
    Task<ResourceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceAssignment>> GetByActivityAsync(Guid activityId,
        CancellationToken ct = default);
    Task<List<ResourceAssignment>> GetByProjectAsync(int projectId,
        CancellationToken ct = default);
    Task<List<ResourceAssignment>> GetByResourceAsync(Guid resourceId,
        CancellationToken ct = default);
    Task<bool> HasAssignmentsForActivityAsync(Guid activityId,
        CancellationToken ct = default);
    Task<bool> HasAssignmentsForResourceAsync(Guid resourceId,
        CancellationToken ct = default);
    Task AddAsync(ResourceAssignment assignment, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ResourceAssignment> assignments,
        CancellationToken ct = default);
    Task UpdateAsync(ResourceAssignment assignment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<decimal> GetTotalCostForActivityAsync(Guid activityId,
        CancellationToken ct = default);
}
```

### IResourceUsageRepository

```csharp
public interface IResourceUsageRepository
{
    Task<List<ResourceUsage>> GetByResourceAsync(Guid resourceId, DateTime? from = null,
        DateTime? to = null, CancellationToken ct = default);
    Task<List<ResourceUsage>> GetByProjectAsync(int projectId, DateTime? from = null,
        DateTime? to = null, ResourceType? typeFilter = null,
        CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ResourceUsage> usages, CancellationToken ct = default);
    Task DeleteByAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
    Task RegenerateForAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
}
```

## Service Contracts

### IResourceService

```csharp
public interface IResourceService
{
    Task<ResourceDto> CreateAsync(CreateResourceRequest request, CancellationToken ct = default);
    Task<ResourceDto> UpdateAsync(UpdateResourceRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task ReactivateAsync(Guid id, CancellationToken ct = default);
    Task<ResourceDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<List<ResourceDto>> SearchAsync(ResourceFilter filter, CancellationToken ct = default);
    Task<ResourceDuplicateCheckResult> CheckDuplicateNameAsync(string name, ResourceScope scope,
        int? projectId, Guid? excludeId = null, CancellationToken ct = default);
}
```

### ICrewService

```csharp
public interface ICrewService
{
    Task<CrewDto> CreateAsync(CreateCrewRequest request, CancellationToken ct = default);
    Task<CrewDto> UpdateAsync(UpdateCrewRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CrewDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<CrewDto>> GetAllAsync(int? projectId = null, CancellationToken ct = default);
    Task<CrewDto> CloneAsync(Guid id, string newName, CancellationToken ct = default);
    Task<decimal> ComputeBlendedRateAsync(Guid crewId, DateTime? rateDate = null,
        CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> ApplyToActivitiesAsync(Guid crewId,
        List<Guid> activityIds, DateTime? startDate = null, DateTime? endDate = null,
        CancellationToken ct = default);
    Task AddResourceToCrewAsync(Guid crewId, Guid resourceId, decimal quantity,
        bool isLead, CancellationToken ct = default);
    Task RemoveResourceFromCrewAsync(Guid crewResourceId, CancellationToken ct = default);
    Task UpdateCrewResourceQuantityAsync(Guid crewResourceId, decimal quantity,
        CancellationToken ct = default);
}
```

### IResourceAssignmentService

```csharp
public interface IResourceAssignmentService
{
    Task<ResourceAssignmentDto> CreateAsync(CreateAssignmentRequest request,
        CancellationToken ct = default);
    Task<ResourceAssignmentDto> UpdateAsync(UpdateAssignmentRequest request,
        CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ResourceAssignmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> GetByActivityAsync(Guid activityId,
        CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> GetByProjectAsync(int projectId,
        CancellationToken ct = default);
    Task<decimal> GetActivityTotalCostAsync(Guid activityId, CancellationToken ct = default);
    Task<bool> ActivityHasAssignmentsAsync(Guid activityId, CancellationToken ct = default);
    Task CheckActivityDeletionAsync(Guid activityId, CancellationToken ct = default);
}
```

### IResourceHistogramService

```csharp
public interface IResourceHistogramService
{
    Task<ResourceHistogramDto> GetHistogramAsync(int projectId, HistogramFilter filter,
        CancellationToken ct = default);
    Task<List<ResourceHistogramDto>> GetByResourceTypeAsync(int projectId,
        ResourceType type, DateTime? from = null, DateTime? to = null,
        CancellationToken ct = default);
    Task<List<ResourceHistogramDto>> GetByResourceAsync(int projectId, Guid resourceId,
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<byte[]> ExportHistogramDataAsync(int projectId, HistogramFilter filter,
        CancellationToken ct = default);
}
```

### IResourceAiEstimationService

```csharp
public interface IResourceAiEstimationService
{
    Task<List<AiSuggestionDto>> EstimateResourcesAsync(Guid activityId,
        CancellationToken ct = default);
    Task<List<ResourceAssignmentDto>> AcceptSuggestionsAsync(Guid activityId,
        List<AcceptedSuggestionDto> suggestions, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
```

### IResourceReportService

```csharp
public interface IResourceReportService
{
    Task<ResourceUsageReportDto> GenerateUsageSummaryAsync(int projectId,
        CancellationToken ct = default);
    Task<ResourceCostReportDto> GenerateCostReportAsync(int projectId,
        CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(int projectId, ReportType type,
        CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(int projectId, ReportType type,
        CancellationToken ct = default);
}
```

### IResourceImportService

```csharp
public interface IResourceImportService
{
    Task<ImportPreviewDto> PreviewImportAsync(Stream fileStream, string fileName,
        int? projectId, CancellationToken ct = default);
    Task<ImportResultDto> ExecuteImportAsync(ImportRequest request,
        CancellationToken ct = default);
}
```
