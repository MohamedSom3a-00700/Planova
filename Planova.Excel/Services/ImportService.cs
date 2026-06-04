using System.Diagnostics;
using Planova.Excel.Models;
using Planova.Excel.Readers;
using Planova.Excel.Validation;
using Serilog;

namespace Planova.Excel.Services;

public class ImportService : IImportService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ImportService>();
    private readonly IWorkbookReader _reader;
    private readonly IValidationService _validationService;

    public ImportService(IWorkbookReader reader, IValidationService validationService)
    {
        _reader = reader;
        _validationService = validationService;
    }

    public async Task<ValidationResult> ValidateAsync(ImportRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var records = await _reader.ReadAllAsync(request.FilePath, request.WorksheetName, ct);
        return await _validationService.ValidateAsync(request.EntityType, records, request.ColumnMappings, ct);
    }

    public async Task<ImportResult> ImportAsync(ImportRequest request, IProgress<int> progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var sw = Stopwatch.StartNew();
        Log.Information("Starting import: EntityType={EntityType}, File={FilePath}, Worksheet={Worksheet}",
            request.EntityType, request.FilePath, request.WorksheetName);

        var records = await _reader.ReadAllAsync(request.FilePath, request.WorksheetName, ct);
        var validationResult = await _validationService.ValidateAsync(request.EntityType, records, request.ColumnMappings, ct);

        var result = new ImportResult
        {
            TotalRecords = records.Count,
            TotalBatches = (int)Math.Ceiling((double)records.Count / request.BatchSize)
        };

        if (!validationResult.IsValid)
        {
            result.Errors = validationResult.Errors;
            result.FailedRecords = validationResult.TotalErrors;
            result.Duration = sw.Elapsed;
            return result;
        }

        var batches = records.Chunk(request.BatchSize).ToList();
        for (int i = 0; i < batches.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var batch = batches[i];

            var (imported, updated, skipped) = await ProcessBatchAsync(request, batch, ct);
            result.ImportedRecords += imported;
            result.UpdatedRecords += updated;
            result.SkippedRecords += skipped;
            result.CompletedBatches = i + 1;

            progress?.Report((int)((double)(i + 1) / batches.Count * 100));
        }

        result.Duration = sw.Elapsed;
        Log.Information("Import completed: Imported={Imported}, Updated={Updated}, Skipped={Skipped}, Failed={Failed}, Duration={Duration}ms",
            result.ImportedRecords, result.UpdatedRecords, result.SkippedRecords, result.FailedRecords, result.Duration.TotalMilliseconds);

        return result;
    }

    public async Task<ImportResult> PreviewImportAsync(ImportRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var records = await _reader.ReadAllAsync(request.FilePath, request.WorksheetName, ct);
        var validationResult = await _validationService.ValidateAsync(request.EntityType, records, request.ColumnMappings, ct);

        return new ImportResult
        {
            TotalRecords = records.Count,
            TotalBatches = (int)Math.Ceiling((double)records.Count / request.BatchSize),
            Errors = validationResult.Errors,
            FailedRecords = validationResult.TotalErrors
        };
    }

    private Task<(int imported, int updated, int skipped)> ProcessBatchAsync(
        ImportRequest request, Dictionary<string, object>[] batch, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int imported = 0, updated = 0, skipped = 0;

        foreach (var record in batch)
        {
            var duplicate = DetectDuplicate(request, record);

            if (duplicate)
            {
                switch (request.DuplicateHandling)
                {
                    case DuplicateStrategy.UpdateAll:
                        updated++;
                        break;
                    case DuplicateStrategy.SkipAll:
                        skipped++;
                        break;
                    case DuplicateStrategy.Cancel:
                        return Task.FromResult((imported, updated, skipped + (batch.Length - imported - updated - skipped)));
                    case DuplicateStrategy.Prompt:
                    default:
                        skipped++;
                        break;
                }
            }
            else
            {
                imported++;
            }
        }

        return Task.FromResult((imported, updated, skipped));
    }

    private static bool DetectDuplicate(ImportRequest request, Dictionary<string, object> record)
    {
        // Simplified duplicate detection based on entity type key fields
        var keyFields = request.EntityType switch
        {
            "Project" => new[] { "Code" },
            "Activity" => new[] { "Code", "ProjectId" },
            "Resource" => new[] { "Name", "Type" },
            "Cost" => new[] { "Description", "Amount" },
            "Risk" => new[] { "Title" },
            _ => Array.Empty<string>()
        };

        // In a full implementation this would query the database
        return false;
    }
}
