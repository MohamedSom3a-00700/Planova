using Planova.Primavera.Application.Dto;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraImportService
{
    Task<XerImportPreviewDto> PreviewAsync(string filePath, CancellationToken ct = default);
    Task<XerImportResultDto> CommitAsync(Guid sessionId, bool overwrite, CancellationToken ct = default);
    Task<List<XerImportSessionDto>> GetImportedSessionsAsync(CancellationToken ct = default);
}
