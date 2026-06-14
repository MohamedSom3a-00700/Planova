using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraWorkspaceRepository
{
    Task<List<PrimaveraActivity>> GetActivitiesAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraRelationship>> GetRelationshipsAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraResourceAssignment>> GetResourceAssignmentsAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraCalendar>> GetCalendarsAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraCode>> GetCodesAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraBaseline>> GetBaselinesAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraUdf>> GetUdfsAsync(int projectId, CancellationToken ct = default);

    Task UpdateActivityAsync(PrimaveraActivity activity, CancellationToken ct = default);
    Task UpdateRelationshipAsync(PrimaveraRelationship relationship, CancellationToken ct = default);
    Task UpdateResourceAssignmentAsync(PrimaveraResourceAssignment assignment, CancellationToken ct = default);
    Task UpdateCalendarAsync(PrimaveraCalendar calendar, CancellationToken ct = default);
    Task UpdateCodeAsync(PrimaveraCode code, CancellationToken ct = default);
    Task UpdateBaselineAsync(PrimaveraBaseline baseline, CancellationToken ct = default);
    Task UpdateUdfAsync(PrimaveraUdf udf, CancellationToken ct = default);

    Task<PrimaveraActivity?> GetActivityByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrimaveraRelationship?> GetRelationshipByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrimaveraResourceAssignment?> GetResourceAssignmentByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrimaveraCalendar?> GetCalendarByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrimaveraCode?> GetCodeByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrimaveraBaseline?> GetBaselineByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrimaveraUdf?> GetUdfByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
