namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsValidationService
{
    Task<IReadOnlyList<ValidationError>> ValidateWbsAsync(Entities.Wbs wbs, CancellationToken ct);
    Task<IReadOnlyList<ValidationError>> ValidateItemAsync(Entities.WbsItem item, IEnumerable<Entities.WbsItem> siblings, CancellationToken ct);
    Task<IReadOnlyList<ValidationError>> ValidateTreeAsync(Guid wbsId, CancellationToken ct);
    bool IsCircularReference(Guid itemId, Guid? newParentId, IEnumerable<Entities.WbsItem> allItems);
    Task<bool> ValidateWeightConsistencyAsync(Guid wbsId, IEnumerable<Entities.WbsItem> allItems, CancellationToken ct);
}

public record ValidationError(string Property, string Message);
