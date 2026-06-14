# Cross-Studio Contracts: Primavera Studio

**Branch**: `011-primavera-studio` | **Date**: 2026-06-12 | **Plan**: [plan.md](../plan.md)

## Contract Pattern

Primavera Studio follows the same cross-studio consumption pattern established by Phase 8 (Reporting Center): **direct nullable injection of domain service interfaces**.

Other studios consume Primavera data by injecting `Planova.Primavera`'s public domain service interfaces directly. No intermediate integration layer, no source resolver, no adapter project.

## Service Interfaces

### IPrimaveraWorkspaceService
Primary contract for other studios to read Primavera schedule data. Supports nullable injection.

```csharp
public interface IPrimaveraWorkspaceService
{
    Task<bool> HasDataAsync(int projectId, CancellationToken ct);
    Task<PrimaveraWorkspaceSnapshot> GetScheduleSnapshotAsync(int projectId, CancellationToken ct);
    Task<PrimaveraActivityDto?> GetActivityAsync(int activityId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraActivityDto>> GetActivitiesAsync(int projectId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraRelationshipDto>> GetRelationshipsAsync(int projectId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraResourceDto>> GetResourceAssignmentsAsync(int projectId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraCalendarDto>> GetCalendarsAsync(int projectId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraBaselineDto>> GetBaselinesAsync(int projectId, CancellationToken ct);
}
```

### IPrimaveraImportService
Handles XER file parsing, preview generation, and staged import commit.

```csharp
public interface IPrimaveraImportService
{
    Task<XerImportPreviewDto> PreviewAsync(string filePath, CancellationToken ct);
    Task<XerImportResultDto> CommitAsync(int previewSessionId, bool overwrite, CancellationToken ct);
    Task<IReadOnlyList<XerImportSessionDto>> GetImportHistoryAsync(int projectId, CancellationToken ct);
}
```

### IPrimaveraExportService
Exports current workspace state to XER format.

```csharp
public interface IPrimaveraExportService
{
    Task<XerExportResultDto> ExportAsync(int projectId, XerExportProfileDto profile, CancellationToken ct);
    Task<XerExportResultDto> ExportAsync(int projectId, string outputPath, CancellationToken ct);
}
```

### IPrimaveraValidationService
On-demand schedule integrity checks.

```csharp
public interface IPrimaveraValidationService
{
    Task<IReadOnlyList<PrimaveraValidationIssueDto>> ValidateAsync(int projectId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraValidationIssueDto>> ValidateEntityAsync(int projectId, PrimaveraEntityType entityType, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraValidationRuleDto>> GetAvailableRulesAsync();
}
```

### IPrimaveraRepairService
Repair suggestions and application.

```csharp
public interface IPrimaveraRepairService
{
    Task<IReadOnlyList<PrimaveraRepairActionDto>> GetSuggestedFixesAsync(int projectId, CancellationToken ct);
    Task<PrimaveraRepairActionDto> ApplyFixAsync(int issueId, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraRepairActionDto>> ApplyFixesBatchAsync(IReadOnlyList<int> issueIds, CancellationToken ct);
    Task<IReadOnlyList<PrimaveraRepairActionDto>> GetRepairHistoryAsync(int projectId, CancellationToken ct);
}
```

## Registration Pattern

```csharp
// In Planova.Primavera.Extensions.ServiceCollectionExtensions
public static IServiceCollection AddPlanovaPrimavera(this IServiceCollection services)
{
    services.AddScoped<IPrimaveraImportService, PrimaveraImportService>();
    services.AddScoped<IPrimaveraExportService, PrimaveraExportService>();
    services.AddScoped<IPrimaveraWorkspaceService, PrimaveraWorkspaceService>();
    services.AddScoped<IPrimaveraValidationService, PrimaveraValidationService>();
    services.AddScoped<IPrimaveraRepairService, PrimaveraRepairService>();
    return services;
}
```

## Consumer Pattern

Consumer modules use nullable injection so they compile and function even when the Primavera module assembly is not loaded:

```csharp
public class ScheduleComparisonService
{
    private readonly IPrimaveraWorkspaceService? _primaveraWorkspace;
    private readonly IActivityService _nativeActivityService;

    public ScheduleComparisonService(
        IPrimaveraWorkspaceService? primaveraWorkspace,
        IActivityService nativeActivityService)
    {
        _primaveraWorkspace = primaveraWorkspace;
        _nativeActivityService = nativeActivityService;
    }

    public async Task<ScheduleData> GetScheduleDataAsync(int projectId, CancellationToken ct)
    {
        if (_primaveraWorkspace != null && await _primaveraWorkspace.HasDataAsync(projectId, ct))
            return await _primaveraWorkspace.GetScheduleSnapshotAsync(projectId, ct);
        return await _nativeActivityService.GetScheduleAsync(projectId, ct);
    }
}
```

## Contract Rationale

- Follows the same proven pattern as `IReportDataProvider<T>` in Phase 8
- Eliminates a dedicated integration service and source resolver
- Keeps fallback decisions local to each consumer
- No direct DB access — all data flows through service interfaces
- Nullable injection naturally handles "Primavera unavailable" without a resolver abstraction
- All DTOs include provenance markers (SourceType, ImportSessionId, SourceFileName)
