// IWbsAiGenerationService — AI-powered WBS structure generation
// Implemented by Planova.Wbs.Application.Services

public interface IWbsAiGenerationService
{
    Task<WbsGenerationResult> GenerateAsync(string projectScope, Guid? referenceBoqId, CancellationToken ct);
    Task<bool> IsAiAvailableAsync(CancellationToken ct);
}
