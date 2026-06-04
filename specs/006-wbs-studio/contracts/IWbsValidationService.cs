// IWbsValidationService — Structural and data validation for WBS entities
// Implemented by Planova.Wbs.Application.Services

public interface IWbsValidationService
{
    Task<IReadOnlyList<ValidationError>> ValidateWbsAsync(Wbs wbs, CancellationToken ct);
    Task<IReadOnlyList<ValidationError>> ValidateItemAsync(WbsItem item, IEnumerable<WbsItem> siblings, CancellationToken ct);
    Task<IReadOnlyList<ValidationError>> ValidateTreeAsync(Guid wbsId, CancellationToken ct);
    bool IsCircularReference(Guid itemId, Guid? newParentId, IEnumerable<WbsItem> allItems);
}
