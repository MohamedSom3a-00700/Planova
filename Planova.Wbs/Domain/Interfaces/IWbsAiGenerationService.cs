using Planova.Wbs.Application.Dto;

namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsAiGenerationService
{
    Task<WbsGenerationResult> GenerateAsync(string projectScope, Guid? referenceBoqId, CancellationToken ct);
    Task<bool> IsAiAvailableAsync(CancellationToken ct);
}
