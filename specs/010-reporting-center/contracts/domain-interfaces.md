# Domain Interfaces — Reporting Center

## Repository Interfaces

### IReportTemplateRepository

```csharp
public interface IReportTemplateRepository
{
    Task<List<ReportTemplate>> GetByProjectAsync(int projectId, ReportType? reportType = null, CancellationToken ct = default);
    Task<List<ReportTemplate>> GetGlobalTemplatesAsync(ReportType? reportType = null, CancellationToken ct = default);
    Task<ReportTemplate?> GetDefaultForProjectAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ReportTemplate template, CancellationToken ct = default);
    Task UpdateAsync(ReportTemplate template, CancellationToken ct = default);
    Task DeleteAsync(ReportTemplate template, CancellationToken ct = default);
}
```

### IReportInstanceRepository

```csharp
public interface IReportInstanceRepository
{
    Task<ReportInstance?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReportInstance>> GetByProjectAsync(int projectId, ReportType? type = null, ReportStatus? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task AddAsync(ReportInstance instance, CancellationToken ct = default);
    Task UpdateAsync(ReportInstance instance, CancellationToken ct = default);
    Task DeleteAsync(ReportInstance instance, CancellationToken ct = default);
}
```

### IReportScheduleRepository

```csharp
public interface IReportScheduleRepository
{
    Task<List<ReportSchedule>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<ReportSchedule?> GetByProjectAndTypeAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReportSchedule>> GetDueSchedulesAsync(DateTime utcNow, CancellationToken ct = default);
    Task AddAsync(ReportSchedule schedule, CancellationToken ct = default);
    Task UpdateAsync(ReportSchedule schedule, CancellationToken ct = default);
    Task DeleteAsync(ReportSchedule schedule, CancellationToken ct = default);
}
```

### IProjectPartyRepository

```csharp
public interface IProjectPartyRepository
{
    Task<List<ProjectParty>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<ProjectParty?> GetClientAsync(int projectId, CancellationToken ct = default);
    Task<ProjectParty?> GetMainContractorAsync(int projectId, CancellationToken ct = default);
    Task<List<ProjectParty>> GetSubContractorsAsync(int projectId, CancellationToken ct = default);
    Task<ProjectParty?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProjectParty party, CancellationToken ct = default);
    Task UpdateAsync(ProjectParty party, CancellationToken ct = default);
    Task DeleteAsync(ProjectParty party, CancellationToken ct = default);
}
```

## Service Interfaces

### IReportEngine

```csharp
public interface IReportEngine
{
    Task<ReportInstanceDto> GenerateAsync(int projectId, ReportType reportType, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
    Task<ReportInstanceDto> RegenerateNarrativeAsync(Guid instanceId, CancellationToken ct = default);
}
```

### IReportDataProvider\<TData\>

```csharp
public interface IReportDataProvider<TData> where TData : class
{
    ReportType HandledType { get; }
    Task<TData> CollectDataAsync(int projectId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
}
```

### IReportSchedulerService

```csharp
public interface IReportSchedulerService
{
    Task<List<ReportScheduleDto>> GetSchedulesAsync(int projectId, CancellationToken ct = default);
    Task<ReportScheduleDto> CreateAsync(CreateScheduleRequest request, CancellationToken ct = default);
    Task<ReportScheduleDto> UpdateAsync(Guid id, UpdateScheduleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<DateTime> ComputeNextRunAsync(ReportSchedule schedule, CancellationToken ct = default);
}
```

### IReportExportService

```csharp
public interface IReportExportService
{
    Task<byte[]> ExportToExcelAsync(Guid instanceId, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(Guid instanceId, CancellationToken ct = default);
    Task<byte[]> ExportToWordAsync(Guid instanceId, CancellationToken ct = default);
    Task<ReportExportDto> SaveExportAsync(Guid instanceId, ExportFormat format, byte[] fileContent, string exportedBy, CancellationToken ct = default);
    Task DeleteExportFileAsync(Guid exportId, CancellationToken ct = default);
}
```

### IReportAiService

```csharp
public interface IReportAiService
{
    Task<string> GenerateNarrativeAsync(ReportType reportType, object reportData, CancellationToken ct = default);
    bool IsAvailable { get; }
}
```

### IProjectPartyService

```csharp
public interface IProjectPartyService
{
    Task<List<ProjectPartyDto>> GetPartiesAsync(int projectId, CancellationToken ct = default);
    Task<ProjectPartyDto> GetClientAsync(int projectId, CancellationToken ct = default);
    Task<ProjectPartyDto> GetMainContractorAsync(int projectId, CancellationToken ct = default);
    Task<List<ProjectPartyDto>> GetSubContractorsAsync(int projectId, CancellationToken ct = default);
    Task<ProjectPartyDto> SavePartyAsync(int projectId, SavePartyRequest request, CancellationToken ct = default);
    Task DeletePartyAsync(Guid partyId, CancellationToken ct = default);
    Task<string> UploadLogoAsync(Guid partyId, string fileName, Stream fileStream, CancellationToken ct = default);
}
```

### IReportSettingsService

```csharp
public interface IReportSettingsService
{
    Task<ReportSettingsDto> GetSettingsAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<ReportSettingsDto> UpdateSettingsAsync(int projectId, ReportType reportType, UpdateSettingsRequest request, CancellationToken ct = default);
    Task ResetToDefaultsAsync(int projectId, ReportType reportType, CancellationToken ct = default);
    Task<List<string>> GetEnabledSectionsAsync(int projectId, ReportType reportType, CancellationToken ct = default);
}
```
