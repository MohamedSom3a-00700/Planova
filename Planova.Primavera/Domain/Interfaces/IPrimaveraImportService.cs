using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraImportService
{
    Task<XerImportPreviewDto> PreviewAsync(string filePath, int projectId = 0, CancellationToken ct = default);
    Task<XerImportResultDto> CommitAsync(Guid sessionId, XerImportType importType, CancellationToken ct = default);
    Task<XerImportResultDto> CancelImportAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<XerImportSessionDto>> GetImportedSessionsAsync(CancellationToken ct = default);
    Task<List<XerImportSessionDto>> GetImportedSessionsByProjectAsync(int projectId, CancellationToken ct = default);
    Task<XerImportSessionDto?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct = default);
    Task DeleteAllXerDataAsync(CancellationToken ct = default);
}
