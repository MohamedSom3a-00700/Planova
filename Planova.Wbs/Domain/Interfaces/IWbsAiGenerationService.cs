using Planova.Wbs.Application.Dto;

namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsAiGenerationService
{
    Task<WbsGenerationResult> GenerateAsync(string projectScope, Guid? referenceBoqId, CancellationToken ct);
    Task<bool> IsAiAvailableAsync(CancellationToken ct);
    Task<WbsAiPreview> GeneratePreviewAsync(WbsAiRequest request, CancellationToken ct);
    Task<WbsAiResult> AcceptGenerationAsync(WbsAiRequest request, Guid previewId, string wbsName, int userId, CancellationToken ct);
}

public enum WbsAiSource { BoqOnly, DocumentsOnly, BoqAndDocuments }

public record WbsAiRequest(Guid ProjectId, WbsAiSource Source, Guid? BoqId, IReadOnlyList<Guid>? DocumentIds);

public record WbsAiPreview(Guid PreviewId, IReadOnlyList<WbsMappingNode> Nodes, int Confidence);

public record WbsAiResult(Guid WbsId, int ItemsCreated);
