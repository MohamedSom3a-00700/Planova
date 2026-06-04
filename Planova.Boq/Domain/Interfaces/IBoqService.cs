using Planova.Boq.Application.Dto;

namespace Planova.Boq.Domain.Interfaces;

public interface IBoqService
{
    Task<BoqDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoqSummaryDto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct);
    Task<BoqDto> CreateAsync(Guid projectId, string name, string currency, CancellationToken ct);
    Task<BoqDto> UpdateAsync(Guid id, string name, string? description, string currency, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoqItemDto>> GetTreeAsync(Guid boqId, CancellationToken ct);
    Task<BoqItemDto> UpdateItemAsync(Guid boqId, Guid itemId, string code, string description, string unit, decimal quantity, decimal rate, string? costCode, bool isActive, CancellationToken ct);
    Task<BoqItemDto> AddItemAsync(Guid boqId, Guid? parentId, string code, string description, string unit, decimal quantity, decimal rate, string? costCode, CancellationToken ct);
    Task DeleteItemAsync(Guid boqId, Guid itemId, CancellationToken ct);
    Task ReorderItemAsync(Guid boqId, Guid itemId, int newSortOrder, CancellationToken ct);
    Task<decimal> ComputeSubtotalAsync(Guid boqId, Guid? parentId, CancellationToken ct);
}
