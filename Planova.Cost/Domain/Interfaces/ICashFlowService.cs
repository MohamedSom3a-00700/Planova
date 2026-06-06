using Planova.Cost.Application.Dto;

namespace Planova.Cost.Domain.Interfaces;

public interface ICashFlowService
{
    Task<List<CashFlowPeriodDto>> GetCashFlowAsync(
        int projectId, CashFlowPeriodType periodType, DateTime? dataDate, CancellationToken ct = default);
}
