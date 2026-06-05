using Planova.Resource.Application.Dto;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceImportService
{
    Task<ImportPreviewDto> PreviewImportAsync(Stream fileStream, string fileName, int? projectId, CancellationToken ct = default);
    Task<ImportResultDto> ExecuteImportAsync(ImportRequest request, CancellationToken ct = default);
}
