// IValidationService — Validates imported records against entity rules before commit
// Implemented by Planova.Excel.Validation

public interface IValidationService
{
    Task<ValidationResult> ValidateAsync(
        string entityType,
        IReadOnlyList<Dictionary<string, object>> records,
        Dictionary<string, string> columnMappings,
        CancellationToken ct);
}
