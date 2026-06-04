// IBoqValidationService — Validates BOQ structural integrity and data quality
// Implemented by Planova.Boq.Application.Services

public interface IBoqValidationService
{
    Task<ValidationResult> ValidateStructureAsync(Guid boqId, CancellationToken ct);
    Task<ValidationResult> ValidateImportAsync(IReadOnlyList<ImportRow> rows, CancellationToken ct);
    Task<ValidationResult> ValidateItemAsync(BoqItem item, CancellationToken ct);
    Task<IReadOnlyList<ValidationIssue>> DetectDuplicatesAsync(Guid boqId, CancellationToken ct);
    Task<IReadOnlyList<ValidationIssue>> DetectCircularReferencesAsync(Guid boqId, CancellationToken ct);
    Task<IReadOnlyList<ValidationIssue>> DetectOrphansAsync(Guid boqId, CancellationToken ct);
}

public record ValidationResult(bool IsValid, IReadOnlyList<ValidationIssue> Errors, IReadOnlyList<ValidationIssue> Warnings);

public record ValidationIssue(
    Guid? ItemId,
    string Code,
    string Field,
    IssueType Type,
    string Message,
    string? SuggestedFix
);

public enum IssueType { Error, Warning }
