using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Enums;

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
    Task BulkStatusChangeAsync(List<Guid> ids, ActivityStatus newStatus, CancellationToken ct = default);
    Task BulkDeleteAsync(List<Guid> ids, CancellationToken ct = default);
    Task BulkCalendarAssignAsync(List<Guid> ids, Guid calendarId, CancellationToken ct = default);
}
