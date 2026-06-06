using Planova.Excel.Models;

namespace Planova.Excel.Readers;

public class ActualCostImportReader
{
    private const string ExpectedActivityCodeColumn = "ActivityCode";
    private const string ExpectedAmountColumn = "Amount";
    private const string ExpectedCurrencyColumn = "Currency";
    private const string ExpectedEntryDateColumn = "EntryDate";

    private static readonly string[] ExpectedColumns =
        { ExpectedActivityCodeColumn, ExpectedAmountColumn, ExpectedCurrencyColumn, ExpectedEntryDateColumn };

    public ValidationResult ValidateSchema(IReadOnlyList<string> columns)
    {
        var errors = new List<ValidationError>();
        var missing = ExpectedColumns
            .Where(exp => !columns.Any(c => string.Equals(c, exp, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missing.Count > 0)
        {
            errors.Add(new ValidationError
            {
                RowIndex = 0,
                ColumnName = string.Join(", ", missing),
                ErrorType = "MissingColumn",
                Message = $"Required columns missing: {string.Join(", ", missing)}"
            });
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            TotalErrors = errors.Count,
            Errors = errors
        };
    }

    public ActualCostRow? ParseRow(Dictionary<string, object> record, int rowIndex)
    {
        if (!record.TryGetValue(ExpectedActivityCodeColumn, out var codeObj) || codeObj == null)
            return null;

        var activityCode = codeObj.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(activityCode))
            return null;

        record.TryGetValue(ExpectedAmountColumn, out var amountObj);
        record.TryGetValue(ExpectedCurrencyColumn, out var currencyObj);
        record.TryGetValue(ExpectedEntryDateColumn, out var dateObj);

        if (!decimal.TryParse(amountObj?.ToString(), out var amount))
            return null;

        DateTime.TryParse(dateObj?.ToString(), out var entryDate);

        return new ActualCostRow
        {
            RowIndex = rowIndex,
            ActivityCode = activityCode,
            Amount = amount,
            Currency = currencyObj?.ToString() ?? "USD",
            EntryDate = entryDate == default ? DateTime.UtcNow : entryDate
        };
    }
}

public class ActualCostRow
{
    public int RowIndex { get; set; }
    public string ActivityCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime EntryDate { get; set; }
}
