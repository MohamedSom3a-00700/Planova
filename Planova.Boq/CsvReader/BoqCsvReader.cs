using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Planova.Boq.Application.Dto;

namespace Planova.Boq.CsvReader;

public class BoqCsvReader : IBoqCsvReader
{
    public async Task<IReadOnlyList<ImportRow>> ReadAsync(string filePath, CsvImportOptions options, CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = options.Delimiter,
            HasHeaderRecord = options.HasHeaders,
            MissingFieldFound = null,
            HeaderValidated = null,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, config);

        await csv.ReadAsync();
        if (options.HasHeaders)
        {
            csv.ReadHeader();
        }

        var rows = new List<ImportRow>();

        while (await csv.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            var raw = new Dictionary<string, object>();
            foreach (var header in csv.HeaderRecord ?? [])
            {
                raw[header] = csv.GetField(header) ?? string.Empty;
            }

            var row = new ImportRow(
                Code: csv.GetField(options.CodeColumn) ?? string.Empty,
                Description: csv.GetField(options.DescriptionColumn) ?? string.Empty,
                Unit: csv.GetField(options.UnitColumn) ?? string.Empty,
                Quantity: csv.GetField<decimal>(options.QuantityColumn),
                Rate: csv.GetField<decimal>(options.RateColumn),
                Level: options.LevelColumn != null ? csv.GetField<int?>(options.LevelColumn) : null,
                ParentId: options.ParentIdColumn != null ? csv.GetField(options.ParentIdColumn) : null,
                ParentCode: null,
                RawValues: raw
            );

            rows.Add(row);
        }

        return rows;
    }

    public async Task<string[]> DetectHeadersAsync(string filePath, string delimiter, CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        return csv.HeaderRecord ?? [];
    }

    public bool CanRead(string filePath) =>
        Path.GetExtension(filePath)?.Equals(".csv", StringComparison.OrdinalIgnoreCase) == true;
}
