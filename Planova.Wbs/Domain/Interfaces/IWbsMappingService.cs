namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsMappingService
{
    Task<WbsMappingPreview> PreviewMappingAsync(
        Guid boqId, WbsMappingMethod method, CancellationToken ct);

    Task<WbsMappingServiceResult> CreateWbsFromMappingAsync(
        Guid boqId, WbsMappingMethod method, string wbsName, int userId, CancellationToken ct);
}

public enum WbsMappingMethod { BySection, ByCsi, ByCostCode, ByTrade, ByDiscipline }

public record WbsMappingPreview(IReadOnlyList<WbsMappingNode> Nodes, int TotalItems, int Depth);

public record WbsMappingNode(string Code, string Name, decimal Weight, IReadOnlyList<WbsMappingNode> Children);

public record WbsMappingServiceResult(Guid WbsId, int ItemsCreated);
