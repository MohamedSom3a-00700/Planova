using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraExportRepository
{
    Task<List<XerRawTable>> GetRawTablesAsync(int projectId, CancellationToken ct = default);
}
