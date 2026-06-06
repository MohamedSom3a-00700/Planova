using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface ICostImportService
{
    Task<ImportResultDto> ImportActualCostsAsync(Stream excelStream, int projectId, CancellationToken ct = default);
}
