using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IReportService
{
    Task<IEnumerable<ProjectSummaryDto>> GetProjectSummaryAsync(CancellationToken ct = default);
    Task<IEnumerable<ClientSummaryDto>> GetClientSummaryAsync(CancellationToken ct = default);
    Task<IEnumerable<ContractSummaryDto>> GetContractSummaryAsync(CancellationToken ct = default);
    Task<byte[]> ExportProjectsPdfAsync(CancellationToken ct = default);
    Task<byte[]> ExportClientsPdfAsync(CancellationToken ct = default);
    Task<byte[]> ExportContractsPdfAsync(CancellationToken ct = default);
}
