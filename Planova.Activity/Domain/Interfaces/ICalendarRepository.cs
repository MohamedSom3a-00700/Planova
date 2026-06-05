using Planova.Activity.Domain.Entities;

namespace Planova.Activity.Domain.Interfaces;

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
