using Planova.Excel.Models;

namespace Planova.Excel.Validation;

public class ValidationService : IValidationService
{
    private readonly Dictionary<string, List<IValidator>> _validators = new()
    {
        ["Project"] = new() { new RequiredFieldValidator("Name"), new RequiredFieldValidator("Code") },
        ["Activity"] = new() { new RequiredFieldValidator("Name"), new RequiredFieldValidator("ProjectId") },
        ["Resource"] = new() { new RequiredFieldValidator("Name"), new RequiredFieldValidator("Type") },
        ["Cost"] = new() { new RequiredFieldValidator("Description"), new RequiredFieldValidator("Amount") },
        ["Risk"] = new() { new RequiredFieldValidator("Title"), new RequiredFieldValidator("Category") }
    };

    public Task<ValidationResult> ValidateAsync(
        string entityType,
        IReadOnlyList<Dictionary<string, object>> records,
        Dictionary<string, string> columnMappings,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var errors = new List<ValidationError>();
        var warnings = new List<string>();

        if (!_validators.ContainsKey(entityType))
        {
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                TotalErrors = 1,
                Errors = new List<ValidationError>
                {
                    new() { RowIndex = 0, ColumnName = "", ErrorType = "InvalidEntity", Message = $"Unknown entity type: {entityType}" }
                }
            });
        }

        var entityValidators = _validators[entityType];

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var mappedRecord = new Dictionary<string, object>();
            foreach (var (excelCol, fieldName) in columnMappings)
            {
                if (record.TryGetValue(excelCol, out var value))
                {
                    mappedRecord[fieldName] = value;
                }
            }

            foreach (var validator in entityValidators)
            {
                var error = validator.Validate(i + 1, mappedRecord);
                if (error is not null)
                {
                    errors.Add(error);
                }
            }
        }

        var result = new ValidationResult
        {
            IsValid = errors.Count == 0,
            TotalErrors = errors.Count,
            Errors = errors,
            Warnings = warnings
        };

        return Task.FromResult(result);
    }
}

public interface IValidator
{
    ValidationError? Validate(int rowIndex, Dictionary<string, object> record);
}

public class RequiredFieldValidator : IValidator
{
    private readonly string _fieldName;

    public RequiredFieldValidator(string fieldName)
    {
        _fieldName = fieldName;
    }

    public ValidationError? Validate(int rowIndex, Dictionary<string, object> record)
    {
        if (!record.TryGetValue(_fieldName, out var value) ||
            value is null ||
            (value is string s && string.IsNullOrWhiteSpace(s)))
        {
            return new ValidationError
            {
                RowIndex = rowIndex,
                ColumnName = _fieldName,
                ErrorType = "Required",
                Message = $"'{_fieldName}' is required and cannot be empty.",
                RawValue = value
            };
        }
        return null;
    }
}
