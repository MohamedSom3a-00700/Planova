using Planova.Activity.Application.Dto;

namespace Planova.Activity.Domain.Interfaces;

public interface IWbsGenerationService
{
    Task<WbsGenerationPreviewDto> PreviewSimpleGenerationAsync(List<Guid> wbsItemIds, CancellationToken ct = default);
    Task<WbsGenerationPreviewDto> PreviewBankGenerationAsync(List<Guid> wbsItemIds, Guid bankId, CancellationToken ct = default);
    Task<List<ActivityDto>> CommitGenerationAsync(WbsGenerationRequest request, CancellationToken ct = default);
    Task<WbsGenerationPreviewDto> ApplyBankToWbsAsync(Guid bankId, List<Guid> wbsItemIds, int projectId, CancellationToken ct = default);
}
