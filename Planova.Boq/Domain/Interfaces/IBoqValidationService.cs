using Planova.Boq.Application.Dto;

namespace Planova.Boq.Domain.Interfaces;

public interface IBoqValidationService
{
    Task<ValidationResult> ValidateStructureAsync(Guid boqId, CancellationToken ct);
    Task<ValidationResult> ValidateImportAsync(IReadOnlyList<ImportRow> rows, CancellationToken ct);
    Task<ValidationResult> ValidateItemAsync(Entities.BoqItem item, CancellationToken ct);
    Task<IReadOnlyList<ValidationIssue>> DetectDuplicatesAsync(Guid boqId, CancellationToken ct);
    Task<IReadOnlyList<ValidationIssue>> DetectCircularReferencesAsync(Guid boqId, CancellationToken ct);
    Task<IReadOnlyList<ValidationIssue>> DetectOrphansAsync(Guid boqId, CancellationToken ct);
}
