using Planova.Excel.Models;

namespace Planova.Excel.Validation;

/// <summary>Validates imported records against entity rules before commit.</summary>
public interface IValidationService
{
    /// <summary>Validates a set of records for the given entity type using the specified column mappings.</summary>
    Task<ValidationResult> ValidateAsync(
        string entityType,
        IReadOnlyList<Dictionary<string, object>> records,
        Dictionary<string, string> columnMappings,
        CancellationToken ct);
}
