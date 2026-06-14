using Planova.Primavera.Application.Models;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraExportService
{
    Task<string> ExportAsync(int projectId, string outputPath, CancellationToken ct = default);
    Task<string> ExportWithProfileAsync(PrimaveraExportProfile profile, CancellationToken ct = default);
}
