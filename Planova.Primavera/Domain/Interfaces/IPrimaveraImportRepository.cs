using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraImportRepository
{
    Task<XerImportSession> CreateSessionAsync(XerImportSession session, CancellationToken ct = default);
    Task<XerImportSession?> GetSessionByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<XerImportSession>> GetSessionsAsync(CancellationToken ct = default);
    Task UpdateSessionAsync(XerImportSession session, CancellationToken ct = default);
    Task SaveRawTablesAsync(IEnumerable<XerRawTable> tables, CancellationToken ct = default);
    Task<PrimaveraProject?> GetProjectByXerIdAsync(string projectId, CancellationToken ct = default);
    Task<bool> HasDuplicateFileAsync(string fileHash, CancellationToken ct = default);
    Task DeleteAllXerDataAsync(CancellationToken ct = default);
    Task<List<XerImportSession>> GetSessionsByProjectAsync(int projectId, CancellationToken ct = default);
    Task PersistImportDataAsync(int projectId, Guid sessionId, List<PrimaveraActivity> activities, List<PrimaveraRelationship> relationships, List<PrimaveraResourceAssignment> resourceAssignments, List<PrimaveraCalendar> calendars, List<PrimaveraCode> codes, List<PrimaveraBaseline> baselines, List<PrimaveraUdf> udfs, CancellationToken ct = default);
}
