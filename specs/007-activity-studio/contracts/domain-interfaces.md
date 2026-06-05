# Domain Interfaces: Activity Studio

## Repository Interfaces

```csharp
namespace Planova.Activity.Domain.Interfaces;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Activity>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<Activity>> GetByWbsItemIdAsync(Guid wbsItemId, CancellationToken ct = default);
    Task<List<Activity>> GetByStatusAsync(int projectId, ActivityStatus status, CancellationToken ct = default);
    Task<List<Activity>> GetChildrenAsync(Guid parentActivityId, CancellationToken ct = default);
    Task<string> GetNextCodeAsync(int projectId, CancellationToken ct = default);
    Task AddAsync(Activity activity, CancellationToken ct = default);
    Task UpdateAsync(Activity activity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

public interface IActivityRelationshipRepository
{
    Task<ActivityRelationship?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityRelationship>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<ActivityRelationship>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default);
    Task AddAsync(ActivityRelationship relationship, CancellationToken ct = default);
    Task UpdateAsync(ActivityRelationship relationship, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid predecessorId, Guid successorId, RelationshipType type, CancellationToken ct = default);
}

public interface ICalendarRepository
{
    Task<Calendar?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Calendar>> GetGlobalCalendarsAsync(CancellationToken ct = default);
    Task<List<Calendar>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<Calendar?> GetDefaultForProjectAsync(int projectId, CancellationToken ct = default);
    Task AddAsync(Calendar calendar, CancellationToken ct = default);
    Task UpdateAsync(Calendar calendar, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ICalendarDayRepository
{
    Task<CalendarDay?> GetAsync(Guid calendarId, DateTime date, CancellationToken ct = default);
    Task<List<CalendarDay>> GetRangeAsync(Guid calendarId, DateTime from, DateTime to, CancellationToken ct = default);
    Task SetWorkingStatusAsync(Guid calendarId, DateTime date, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task BulkSetRangeAsync(Guid calendarId, DateTime from, DateTime to, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IActivityBankRepository
{
    Task<ActivityBank?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityBank>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<List<ActivityBank>> SearchAsync(string query, CancellationToken ct = default);
    Task AddAsync(ActivityBank bank, CancellationToken ct = default);
    Task UpdateAsync(ActivityBank bank, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsStandardAsync(Guid id, CancellationToken ct = default);
}

public interface IActivityBankItemRepository
{
    Task<List<ActivityBankItem>> GetByBankIdAsync(Guid bankId, CancellationToken ct = default);
    Task<ActivityBankItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ActivityBankItem> items, CancellationToken ct = default);
    Task UpdateAsync(ActivityBankItem item, CancellationToken ct = default);
    Task DeleteByBankIdAsync(Guid bankId, CancellationToken ct = default);
}

public interface IActivityBankItemRelationshipRepository
{
    Task<List<ActivityBankItemRelationship>> GetByBankIdAsync(Guid bankId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ActivityBankItemRelationship> relationships, CancellationToken ct = default);
    Task DeleteByBankIdAsync(Guid bankId, CancellationToken ct = default);
}
```

## Service Interfaces

```csharp
namespace Planova.Activity.Domain.Interfaces;

public interface IActivityService
{
    Task<ActivityDto> CreateAsync(CreateActivityRequest request, CancellationToken ct = default);
    Task<ActivityDto> UpdateAsync(UpdateActivityRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ActivityDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityDto>> GetByProjectAsync(int projectId, ActivityFilter? filter = null, CancellationToken ct = default);
    Task<List<ActivityDto>> GetByWbsItemAsync(Guid wbsItemId, CancellationToken ct = default);
    Task<ActivityDto> ChangeStatusAsync(Guid id, ActivityStatus newStatus, string? reason = null, CancellationToken ct = default);
    Task<List<ActivityDto>> GetWbsSummaryChildrenAsync(Guid parentActivityId, CancellationToken ct = default);
    Task RecalculateWbsSummaryAsync(Guid activityId, CancellationToken ct = default);
}

public interface IActivityRelationshipService
{
    Task<ActivityRelationshipDto> CreateAsync(CreateRelationshipRequest request, CancellationToken ct = default);
    Task<ActivityRelationshipDto> UpdateAsync(UpdateRelationshipRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityRelationshipDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<List<ActivityRelationshipDto>> GetByActivityAsync(Guid activityId, CancellationToken ct = default);
    Task<CircularReferenceCheckResult> ValidateNewRelationshipAsync(Guid predecessorId, Guid successorId, CancellationToken ct = default);
}

public interface ICalendarService
{
    Task<CalendarDto> CreateAsync(CreateCalendarRequest request, CancellationToken ct = default);
    Task<CalendarDto> UpdateAsync(UpdateCalendarRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<CalendarDto>> GetCalendarsAsync(int? projectId = null, CancellationToken ct = default);
    Task<CalendarDto?> GetDefaultAsync(int projectId, CancellationToken ct = default);
    Task<CalendarDayDto> SetDayStatusAsync(Guid calendarId, DateTime date, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task BulkSetDaysAsync(Guid calendarId, DateTime from, DateTime to, CalendarDayStatus status, string? label = null, CancellationToken ct = default);
    Task<List<CalendarDayDto>> GetDayRangeAsync(Guid calendarId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<DateTime> AddWorkingDaysAsync(DateTime startDate, int days, Guid? calendarId = null, int? projectId = null, CancellationToken ct = default);
    Task<int> CountWorkingDaysAsync(DateTime from, DateTime to, Guid? calendarId = null, int? projectId = null, CancellationToken ct = default);
}

public interface IActivityBankService
{
    Task<ActivityBankDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityBankDto>> BrowseAsync(string? category = null, string? search = null, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ActivityBankDto> CreateCustomAsync(CreateBankEntryRequest request, CancellationToken ct = default);
    Task<ActivityBankDto> UpdateCustomAsync(UpdateBankEntryRequest request, CancellationToken ct = default);
    Task DeleteCustomAsync(Guid id, CancellationToken ct = default);
    Task SeedIfEmptyAsync(CancellationToken ct = default);
}

public interface IWbsGenerationService
{
    Task<WbsGenerationPreviewDto> PreviewSimpleGenerationAsync(List<Guid> wbsItemIds, CancellationToken ct = default);
    Task<WbsGenerationPreviewDto> PreviewBankGenerationAsync(List<Guid> wbsItemIds, Guid bankId, CancellationToken ct = default);
    Task<List<ActivityDto>> CommitGenerationAsync(WbsGenerationRequest request, CancellationToken ct = default);
    // Warn + offer replace/merge when activities already exist under a WBS item (FR-018)
}

public interface IActivityReportService
{
    Task<ScheduleReportDto> GenerateScheduleReportAsync(int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(int projectId, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(int projectId, CancellationToken ct = default);
}
```
