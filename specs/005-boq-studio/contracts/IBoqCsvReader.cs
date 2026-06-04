// IBoqCsvReader — Reads CSV files for standalone BOQ import path
// Implemented by Planova.Boq.CsvReader

public interface IBoqCsvReader
{
    Task<IReadOnlyList<ImportRow>> ReadAsync(string filePath, CsvImportOptions options, CancellationToken ct);
    Task<string[]> DetectHeadersAsync(string filePath, string delimiter, CancellationToken ct);
    bool CanRead(string filePath);
}
