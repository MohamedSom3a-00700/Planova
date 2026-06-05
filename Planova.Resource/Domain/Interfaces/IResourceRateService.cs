using Planova.Resource.Application.Dto;

namespace Planova.Resource.Domain.Interfaces;

public interface IResourceRateService
{
    Task<ResourceRateDto> AddRateAsync(CreateRateRequest request, CancellationToken ct = default);
    Task<ResourceRateDto> UpdateRateAsync(Guid id, decimal rate, string currency, string unitOfMeasure, bool isDefault, string? notes, CancellationToken ct = default);
    Task DeleteRateAsync(Guid id, CancellationToken ct = default);
    Task<List<ResourceRateDto>> GetRateHistoryAsync(Guid resourceId, CancellationToken ct = default);
    Task<ResourceRateDto?> GetEffectiveRateAsync(Guid resourceId, DateTime date, CancellationToken ct = default);
    Task<List<ResourceRateDto>> BulkUpdateRatesAsync(List<Guid> resourceIds, decimal newRate, DateTime effectiveDate, string currency, string unitOfMeasure, CancellationToken ct = default);
}
